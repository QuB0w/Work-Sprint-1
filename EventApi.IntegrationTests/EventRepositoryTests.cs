using Microsoft.Extensions.DependencyInjection;
using Sprint_1_WebAPI.DataAccess.Repositories;
using Sprint_1_WebAPI.Models;

namespace EventApi.IntegrationTests;

[Collection("PostgreSql")]
public sealed class EventRepositoryTests : IDisposable
{
    private readonly PostgreSqlFixture _fixture;
    private ServiceProvider? _serviceProvider;

    public EventRepositoryTests(PostgreSqlFixture fixture)
    {
        _fixture = fixture;
    }

    public void Dispose()
    {
        _serviceProvider?.Dispose();
    }

    [Fact]
    public async Task AddAsync_PersistsEvent()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        var eventItem = CreateEvent("Add test", totalSeats: 15);
        var persisted = await repository.AddAsync(eventItem);
        var fromDb = await repository.GetByIdAsNoTrackingAsync(persisted.Id);

        Assert.NotNull(fromDb);
        Assert.Equal("Add test", fromDb.Title);
        Assert.Equal(15, fromDb.TotalSeats);
        Assert.Equal(15, fromDb.AvailableSeats);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTrackedEvent()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        var eventItem = await repository.AddAsync(CreateEvent("Tracked test"));
        var fromDb = await repository.GetByIdAsync(eventItem.Id);

        Assert.NotNull(fromDb);
        Assert.Equal(eventItem.Id, fromDb.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotFound()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        var fromDb = await repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(fromDb);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsEventsOrderedByStartAt()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        var first = await repository.AddAsync(CreateEvent("First", startAt: DateTime.UtcNow.AddDays(2)));
        var second = await repository.AddAsync(CreateEvent("Second", startAt: DateTime.UtcNow.AddDays(1)));
        var third = await repository.AddAsync(CreateEvent("Third", startAt: DateTime.UtcNow.AddDays(3)));

        var all = await repository.GetAllAsync();
        var ids = all.Select(e => e.Id).ToList();

        Assert.Equal(new[] { second.Id, first.Id, third.Id }, ids);
    }

    [Fact]
    public async Task GetFilteredAsync_FiltersByTitle_CaseInsensitive()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        await repository.AddAsync(CreateEvent("Alpha Conference"));
        await repository.AddAsync(CreateEvent("Beta Meetup"));
        await repository.AddAsync(CreateEvent("Another Alpha Event"));

        var result = await repository.GetFilteredAsync(title: "alpha", null, null, 1, 10);

        Assert.Equal(2, result.TotalCount);
        Assert.All(result.Items, e => Assert.Contains("alpha", e.Title, StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public async Task GetFilteredAsync_FiltersByFromDate()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        var past = await repository.AddAsync(CreateEvent("Past", startAt: DateTime.UtcNow.AddDays(-2)));
        var future = await repository.AddAsync(CreateEvent("Future", startAt: DateTime.UtcNow.AddDays(2)));

        var result = await repository.GetFilteredAsync(null, DateTime.UtcNow.AddDays(1), null, 1, 10);

        Assert.Single(result.Items);
        Assert.Equal(future.Id, result.Items.Single().Id);
    }

    [Fact]
    public async Task GetFilteredAsync_FiltersByToDate()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        var past = await repository.AddAsync(CreateEvent("Past", endAt: DateTime.UtcNow.AddDays(-1)));
        var future = await repository.AddAsync(CreateEvent("Future", endAt: DateTime.UtcNow.AddDays(3)));

        var result = await repository.GetFilteredAsync(null, null, DateTime.UtcNow.AddDays(1), 1, 10);

        Assert.Single(result.Items);
        Assert.Equal(past.Id, result.Items.Single().Id);
    }

    [Fact]
    public async Task GetFilteredAsync_AppliesCombinedFilters()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        var title = "Combined";
        var fromDate = DateTime.UtcNow.AddDays(1);
        var toDate = DateTime.UtcNow.AddDays(5);

        await repository.AddAsync(CreateEvent(title, startAt: fromDate.AddDays(-2), endAt: fromDate.AddDays(-1)));
        await repository.AddAsync(CreateEvent(title, startAt: fromDate.AddDays(1), endAt: toDate.AddDays(1)));
        await repository.AddAsync(CreateEvent("Other", startAt: fromDate.AddDays(1), endAt: toDate.AddDays(1)));

        var result = await repository.GetFilteredAsync(title, fromDate, toDate, 1, 10);

        Assert.Single(result.Items);
        Assert.Equal(title, result.Items.Single().Title);
    }

    [Fact]
    public async Task GetFilteredAsync_PaginatesResults()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        for (int i = 0; i < 5; i++)
        {
            await repository.AddAsync(CreateEvent($"Event {i}", startAt: DateTime.UtcNow.AddHours(i)));
        }

        var firstPage = await repository.GetFilteredAsync(null, null, null, 1, 2);
        var secondPage = await repository.GetFilteredAsync(null, null, null, 2, 2);

        Assert.Equal(2, firstPage.Items.Count);
        Assert.Equal(2, secondPage.Items.Count);
        Assert.Equal(5, firstPage.TotalCount);
        Assert.Equal(3, firstPage.TotalPages);
        Assert.True(firstPage.HasNextPage);
        Assert.True(secondPage.HasPreviousPage);
        Assert.True(secondPage.HasNextPage);
    }

    [Fact]
    public async Task UpdateAsync_PersistsChanges()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        var eventItem = await repository.AddAsync(CreateEvent("Original"));
        eventItem.Title = "Updated";
        eventItem.Description = "New description";
        eventItem.StartAt = DateTime.UtcNow.AddDays(7);

        await repository.UpdateAsync(eventItem);
        var fromDb = await repository.GetByIdAsNoTrackingAsync(eventItem.Id);

        Assert.NotNull(fromDb);
        Assert.Equal("Updated", fromDb.Title);
        Assert.Equal("New description", fromDb.Description);
        Assert.Equal(eventItem.StartAt, fromDb.StartAt);
    }

    [Fact]
    public async Task DeleteAsync_RemovesEventAndCascadesBookings()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();

        var eventItem = await eventRepository.AddAsync(CreateEvent("Cascade test", totalSeats: 1));
        var booking = Booking.CreatePending(eventItem.Id);
        await bookingRepository.AddAsync(booking);

        var deleted = await eventRepository.DeleteAsync(eventItem.Id);
        var eventFromDb = await eventRepository.GetByIdAsNoTrackingAsync(eventItem.Id);
        var bookingFromDb = await bookingRepository.GetByIdAsNoTrackingAsync(booking.Id);

        Assert.True(deleted);
        Assert.Null(eventFromDb);
        Assert.Null(bookingFromDb);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenEventNotFound()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        var deleted = await repository.DeleteAsync(Guid.NewGuid());

        Assert.False(deleted);
    }

    [Fact]
    public async Task ClearAllAsync_RemovesAllEvents()
    {
        var services = await CreateServicesAsync();
        using var scope = services.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

        await repository.AddAsync(CreateEvent("One"));
        await repository.AddAsync(CreateEvent("Two"));
        await repository.ClearAllAsync();

        var all = await repository.GetAllAsync();

        Assert.Empty(all);
    }

    private async Task<ServiceProvider> CreateServicesAsync()
    {
        var services = await _fixture.CreateServiceProviderAsync();
        _serviceProvider = services;
        return services;
    }

    private static Event CreateEvent(
        string title,
        int totalSeats = 10,
        DateTime? startAt = null,
        DateTime? endAt = null)
    {
        var start = startAt ?? DateTime.UtcNow.AddDays(1);
        var end = endAt ?? start.AddHours(1);

        return Event.Create(new CreateEventRequest
        {
            Title = title,
            Description = "Test description",
            StartAt = start,
            EndAt = end,
            TotalSeats = totalSeats
        });
    }
}
