using Interfaces;
using Microsoft.Extensions.DependencyInjection;
using Sprint_1_WebAPI.Exceptions;
using Sprint_1_WebAPI.Models;
using Xunit;

namespace EventServiceUnitTests;

public class EfCoreServiceTests : IClassFixture<EfServiceTestFixture>
{
    private readonly ServiceProvider _serviceProvider;

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

        var booking = await bookingService.CreateBookingAsync(eventItem.Id);
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
        await bookingService.CreateBookingAsync(eventItem.Id);

        await Assert.ThrowsAsync<NoAvailableSeatsException>(() => bookingService.CreateBookingAsync(eventItem.Id));
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
                    return await bookingService.CreateBookingAsync(eventId);
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
        var booking = await bookingService.CreateBookingAsync(eventItem.Id);

        await bookingService.ConfirmBookingAsync(booking!.Id);
        var result = await bookingService.GetBookingByIdAsync(booking.Id);

        Assert.Equal(BookingStatus.Confirmed, result!.Status);
        Assert.NotNull(result.ProcessedAt);
    }

    private static CreateEventRequest CreateEventRequest(int totalSeats = 10)
    {
        return new CreateEventRequest
        {
            Title = "Test event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            TotalSeats = totalSeats
        };
    }
}
