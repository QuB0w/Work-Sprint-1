using EventApi.Application.DTOs;
using EventApi.Application.Interfaces;
using EventApi.Domain.Enums;
using EventApi.Domain.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventServiceUnitTests;

public class EfCoreServiceTests : IClassFixture<EfServiceTestFixture>
{
    private readonly ServiceProvider _serviceProvider;
    private static readonly Guid TestUserId = Guid.NewGuid();

    public EfCoreServiceTests(EfServiceTestFixture fixture)
    {
        _serviceProvider = fixture.ServiceProvider;
    }

    [Fact]
    public async Task CreateEventAsync_PersistsEventWithInitialSeatCapacity()
    {
        using var scope = _serviceProvider.CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();

        var createdEvent = await eventService.CreateEventAsync(CreateEventRequest(totalSeats: 3));
        var result = await eventService.GetEventByIdAsync(createdEvent.Id);

        Assert.NotNull(result);
        Assert.Equal(3, result.TotalSeats);
        Assert.Equal(3, result.AvailableSeats);
    }

    [Fact]
    public async Task CreateBookingAsync_ReservesOneSeatAndPersistsBooking()
    {
        using var scope = _serviceProvider.CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var eventItem = await eventService.CreateEventAsync(CreateEventRequest(totalSeats: 2));

        var booking = await bookingService.CreateBookingAsync(eventItem.Id, TestUserId);
        var updatedEvent = await eventService.GetEventByIdAsync(eventItem.Id);

        Assert.NotNull(booking);
        Assert.Equal(1, updatedEvent!.AvailableSeats);
        Assert.Equal(BookingStatus.Pending, booking.Status);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenSeatsAreExhausted_ThrowsNoAvailableSeatsException()
    {
        using var scope = _serviceProvider.CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var eventItem = await eventService.CreateEventAsync(CreateEventRequest(totalSeats: 1));
        await bookingService.CreateBookingAsync(eventItem.Id, TestUserId);

        await Assert.ThrowsAsync<NoAvailableSeatsException>(
            () => bookingService.CreateBookingAsync(eventItem.Id, Guid.NewGuid()));
    }

    [Fact]
    public async Task CreateBookingAsync_PastEvent_ThrowsEventAlreadyStartedException()
    {
        using var scope = _serviceProvider.CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var pastRequest = new CreateEventRequest
        {
            Title = "Past event",
            StartAt = DateTime.UtcNow.AddHours(-2),
            EndAt = DateTime.UtcNow.AddHours(-1),
            TotalSeats = 10
        };
        var eventItem = await eventService.CreateEventAsync(pastRequest);

        await Assert.ThrowsAsync<EventAlreadyStartedException>(
            () => bookingService.CreateBookingAsync(eventItem.Id, TestUserId));
    }

    [Fact]
    public async Task CreateBookingAsync_ExceedsActiveBookingLimit_ThrowsActiveBookingLimitExceededException()
    {
        using var scope = _serviceProvider.CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var userId = Guid.NewGuid();
        for (int i = 0; i < ActiveBookingLimitExceededException.MaxActiveBookings; i++)
        {
            var ev = await eventService.CreateEventAsync(CreateEventRequest(totalSeats: 100));
            await bookingService.CreateBookingAsync(ev.Id, userId);
        }

        var lastEvent = await eventService.CreateEventAsync(CreateEventRequest(totalSeats: 100));
        await Assert.ThrowsAsync<ActiveBookingLimitExceededException>(
            () => bookingService.CreateBookingAsync(lastEvent.Id, userId));
    }

    [Fact]
    public async Task CreateBookingAsync_DifferentUsersLimitsAreIndependent()
    {
        using var scope = _serviceProvider.CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var user1 = Guid.NewGuid();
        var user2 = Guid.NewGuid();

        for (int i = 0; i < ActiveBookingLimitExceededException.MaxActiveBookings; i++)
        {
            var ev = await eventService.CreateEventAsync(CreateEventRequest(totalSeats: 100));
            await bookingService.CreateBookingAsync(ev.Id, user1);
        }

        var eventForUser2 = await eventService.CreateEventAsync(CreateEventRequest(totalSeats: 100));
        var booking = await bookingService.CreateBookingAsync(eventForUser2.Id, user2);
        Assert.NotNull(booking);
    }

    [Fact]
    public async Task CreateBookingAsync_ConcurrentRequests_DoNotOverbook()
    {
        const int totalSeats = 5;
        const int concurrentRequests = 20;

        Guid eventId;
        using (var scope = _serviceProvider.CreateScope())
        {
            var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
            eventId = (await eventService.CreateEventAsync(CreateEventRequest(totalSeats))).Id;
        }

        var tasks = Enumerable.Range(0, concurrentRequests)
            .Select(_ => Task.Run(async () =>
            {
                using var scope = _serviceProvider.CreateScope();
                var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
                try
                {
                    return await bookingService.CreateBookingAsync(eventId, Guid.NewGuid());
                }
                catch (NoAvailableSeatsException)
                {
                    return null;
                }
            }));

        var bookings = await Task.WhenAll(tasks);

        using var verificationScope = _serviceProvider.CreateScope();
        var verificationEventService = verificationScope.ServiceProvider.GetRequiredService<IEventService>();
        var updatedEvent = await verificationEventService.GetEventByIdAsync(eventId);

        Assert.Equal(totalSeats, bookings.Count(booking => booking is not null));
        Assert.Equal(0, updatedEvent!.AvailableSeats);
    }

    [Fact]
    public async Task ConfirmBookingAsync_ChangesPersistedStatus()
    {
        using var scope = _serviceProvider.CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();
        var eventItem = await eventService.CreateEventAsync(CreateEventRequest());
        var booking = await bookingService.CreateBookingAsync(eventItem.Id, TestUserId);

        await bookingService.ConfirmBookingAsync(booking!.Id);
        var result = await bookingService.GetBookingByIdAsync(booking.Id);

        Assert.Equal(BookingStatus.Confirmed, result!.Status);
        Assert.NotNull(result.ProcessedAt);
    }

    [Fact]
    public async Task CancelBookingAsync_OtherUser_ThrowsForbiddenException()
    {
        using var scope = _serviceProvider.CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var owner = Guid.NewGuid();
        var stranger = Guid.NewGuid();
        var eventItem = await eventService.CreateEventAsync(CreateEventRequest());
        var booking = await bookingService.CreateBookingAsync(eventItem.Id, owner);

        await Assert.ThrowsAsync<ForbiddenException>(
            () => bookingService.CancelBookingAsync(booking!.Id, stranger, "User"));
    }

    [Fact]
    public async Task CancelBookingAsync_AdminCanCancelAnyBooking()
    {
        using var scope = _serviceProvider.CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var owner = Guid.NewGuid();
        var admin = Guid.NewGuid();
        var eventItem = await eventService.CreateEventAsync(CreateEventRequest());
        var booking = await bookingService.CreateBookingAsync(eventItem.Id, owner);

        await bookingService.CancelBookingAsync(booking!.Id, admin, "Admin");

        var result = await bookingService.GetBookingByIdAsync(booking.Id);
        Assert.Equal(BookingStatus.Cancelled, result!.Status);
    }

    [Fact]
    public async Task CancelBookingAsync_OwnerCanCancelOwnBooking()
    {
        using var scope = _serviceProvider.CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var owner = Guid.NewGuid();
        var eventItem = await eventService.CreateEventAsync(CreateEventRequest());
        var booking = await bookingService.CreateBookingAsync(eventItem.Id, owner);

        await bookingService.CancelBookingAsync(booking!.Id, owner, "User");

        var result = await bookingService.GetBookingByIdAsync(booking.Id);
        Assert.Equal(BookingStatus.Cancelled, result!.Status);
        Assert.NotNull(result.ProcessedAt);
    }

    [Fact]
    public async Task CancelBookingAsync_ReleasesSeatBackToEvent()
    {
        using var scope = _serviceProvider.CreateScope();
        var eventService = scope.ServiceProvider.GetRequiredService<IEventService>();
        var bookingService = scope.ServiceProvider.GetRequiredService<IBookingService>();

        var owner = Guid.NewGuid();
        var eventItem = await eventService.CreateEventAsync(CreateEventRequest(totalSeats: 2));
        var booking = await bookingService.CreateBookingAsync(eventItem.Id, owner);

        var beforeCancel = await eventService.GetEventByIdAsync(eventItem.Id);
        Assert.Equal(1, beforeCancel!.AvailableSeats);

        await bookingService.CancelBookingAsync(booking!.Id, owner, "User");

        var afterCancel = await eventService.GetEventByIdAsync(eventItem.Id);
        Assert.Equal(2, afterCancel!.AvailableSeats);
    }

    private static CreateEventRequest CreateEventRequest(int totalSeats = 10)
    {
        return new CreateEventRequest
        {
            Title = "Test event",
            StartAt = DateTime.UtcNow.AddDays(1),
            EndAt = DateTime.UtcNow.AddDays(1).AddHours(1),
            TotalSeats = totalSeats
        };
    }
}
