using EventApi.Application.Interfaces;
using EventApi.Domain.Entities;
using EventApi.Domain.Enums;
using Microsoft.Extensions.DependencyInjection;

namespace EventApi.IntegrationTests;

[Collection("PostgreSql")]
public sealed class BookingRepositoryTests : IDisposable
{
    private readonly PostgreSqlFixture _fixture;
    private ServiceProvider? _serviceProvider;

    public BookingRepositoryTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public async Task AddAsync_PersistsBooking()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await userRepository.AddAsync(CreateUser("bookingadd"));
        var eventItem = await eventRepository.AddAsync(CreateEvent("Booking add"));
        var booking = Booking.CreatePending(eventItem.Id, user.Id);
        var persisted = await bookingRepository.AddAsync(booking);
        var fromDb = await bookingRepository.GetByIdAsNoTrackingAsync(persisted.Id);

        Assert.NotNull(fromDb);
        Assert.Equal(eventItem.Id, fromDb.EventId);
        Assert.Equal(user.Id, fromDb.UserId);
        Assert.Equal(BookingStatus.Pending, fromDb.Status);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTrackedBooking()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await userRepository.AddAsync(CreateUser("bookingget"));
        var eventItem = await eventRepository.AddAsync(CreateEvent("Booking get"));
        var booking = await bookingRepository.AddAsync(Booking.CreatePending(eventItem.Id, user.Id));

        var fromDb = await bookingRepository.GetByIdAsync(booking.Id);

        Assert.NotNull(fromDb);
        Assert.Equal(booking.Id, fromDb.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

        var fromDb = await bookingRepository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(fromDb);
    }

    [Fact]
    public async Task GetPendingBookingIdsAsync_ReturnsOnlyPendingBookings()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await userRepository.AddAsync(CreateUser("pendingfilter"));
        var eventItem = await eventRepository.AddAsync(CreateEvent("Pending filter"));
        var pending = await bookingRepository.AddAsync(Booking.CreatePending(eventItem.Id, user.Id));
        var confirmed = await bookingRepository.AddAsync(Booking.CreatePending(eventItem.Id, user.Id));
        confirmed.Confirm();
        await bookingRepository.UpdateAsync(confirmed);

        var pendingIds = await bookingRepository.GetPendingBookingIdsAsync();

        Assert.Single(pendingIds);
        Assert.Contains(pending.Id, pendingIds);
        Assert.DoesNotContain(confirmed.Id, pendingIds);
    }

    [Fact]
    public async Task UpdateAsync_PersistsStatusChange()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await userRepository.AddAsync(CreateUser("updatestatus"));
        var eventItem = await eventRepository.AddAsync(CreateEvent("Update status"));
        var booking = await bookingRepository.AddAsync(Booking.CreatePending(eventItem.Id, user.Id));

        booking.Confirm();
        await bookingRepository.UpdateAsync(booking);

        var fromDb = await bookingRepository.GetByIdAsNoTrackingAsync(booking.Id);

        Assert.NotNull(fromDb);
        Assert.Equal(BookingStatus.Confirmed, fromDb.Status);
        Assert.NotNull(fromDb.ProcessedAt);
    }

    [Fact]
    public async Task DeleteAsync_RemovesBooking()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await userRepository.AddAsync(CreateUser("deletebooking"));
        var eventItem = await eventRepository.AddAsync(CreateEvent("Delete booking"));
        var booking = await bookingRepository.AddAsync(Booking.CreatePending(eventItem.Id, user.Id));

        var deleted = await bookingRepository.DeleteAsync(booking.Id);
        var fromDb = await bookingRepository.GetByIdAsNoTrackingAsync(booking.Id);

        Assert.True(deleted);
        Assert.Null(fromDb);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenBookingNotFound()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

        var deleted = await bookingRepository.DeleteAsync(Guid.NewGuid());

        Assert.False(deleted);
    }

    [Fact]
    public async Task ClearAllAsync_RemovesAllBookings()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var userRepository = scope.ServiceProvider.GetRequiredService<IUserRepository>();

        var user = await userRepository.AddAsync(CreateUser("clearbookings"));
        var eventItem = await eventRepository.AddAsync(CreateEvent("Clear bookings"));
        await bookingRepository.AddAsync(Booking.CreatePending(eventItem.Id, user.Id));
        await bookingRepository.AddAsync(Booking.CreatePending(eventItem.Id, user.Id));
        await bookingRepository.ClearAllAsync();

        var pendingIds = await bookingRepository.GetPendingBookingIdsAsync();

        Assert.Empty(pendingIds);
    }

    private async Task<ServiceProvider> CreateServicesAsync()
    {
        var services = await _fixture.CreateServiceProviderAsync();
        _serviceProvider = services;
        return services;
    }

    private static Event CreateEvent(string title)
    {
        var start = DateTime.UtcNow.AddDays(1);
        return Event.Create(title, "Test description", start, start.AddHours(1), 10);
    }

    private static User CreateUser(string login)
    {
        return User.Create(login, "testhash", EventApi.Domain.Enums.UserRole.User);
    }
}
