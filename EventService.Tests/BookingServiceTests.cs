using Interfaces;
using Sprint_1_WebAPI.DataAccess;
using Sprint_1_WebAPI.Exceptions;
using Sprint_1_WebAPI.Models;
using Xunit;

namespace EventServiceUnitTests;

[Collection("Sequential")]
public class BookingServiceTests
{
    private readonly IEventService _eventService;
    private readonly IBookingStore _bookingStore;
    private readonly IEventStore _eventStore;
    private readonly IBookingService _bookingService;

    public BookingServiceTests()
    {
        _eventStore = new InMemoryEventStore();
        _eventService = new EventService();
        _bookingStore = new InMemoryBookingStore();
        _bookingService = new BookingService(_eventService, _bookingStore, _eventStore);
    }

    private void Setup()
    {
        _eventService.ClearAllEvents();
        _bookingService.ClearAllBookings();
    }

    private Event CreateTestEvent(int totalSeats = 10)
    {
        return _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Test Event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1),
            TotalSeats = totalSeats
        });
    }

    [Fact]
    public async Task CreateBookingAsync_WithExistingEvent_ShouldReturnPendingBooking()
    {
        // Arrange
        Setup();
        var createdEvent = CreateTestEvent();

        // Act
        var result = await _bookingService.CreateBookingAsync(createdEvent.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdEvent.Id, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
        Assert.NotEqual(Guid.Empty, result.Id);
        Assert.True(result.CreatedAt > DateTime.MinValue);
    }

    [Fact]
    public async Task CreateBookingAsync_MultipleBookingsForSameEvent_ShouldHaveUniqueIds()
    {
        // Arrange
        Setup();
        var createdEvent = CreateTestEvent(totalSeats: 3);

        // Act
        var booking1 = await _bookingService.CreateBookingAsync(createdEvent.Id);
        var booking2 = await _bookingService.CreateBookingAsync(createdEvent.Id);
        var booking3 = await _bookingService.CreateBookingAsync(createdEvent.Id);

        // Assert
        Assert.NotNull(booking1);
        Assert.NotNull(booking2);
        Assert.NotNull(booking3);
        Assert.NotEqual(booking1.Id, booking2.Id);
        Assert.NotEqual(booking2.Id, booking3.Id);
        Assert.NotEqual(booking1.Id, booking3.Id);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WithValidId_ShouldReturnBooking()
    {
        // Arrange
        Setup();
        var createdEvent = CreateTestEvent();
        var createdBooking = await _bookingService.CreateBookingAsync(createdEvent.Id);

        // Act
        var result = await _bookingService.GetBookingByIdAsync(createdBooking!.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdBooking.Id, result.Id);
        Assert.Equal(createdBooking.EventId, result.EventId);
        Assert.Equal(BookingStatus.Pending, result.Status);
    }

    [Fact]
    public async Task GetBookingByIdAsync_AfterConfirm_ShouldReturnConfirmedStatus()
    {
        // Arrange
        Setup();
        var createdEvent = CreateTestEvent();
        var createdBooking = await _bookingService.CreateBookingAsync(createdEvent.Id);

        // Act
        await _bookingService.ConfirmBookingAsync(createdBooking!.Id);
        var result = await _bookingService.GetBookingByIdAsync(createdBooking.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(BookingStatus.Confirmed, result.Status);
        Assert.NotNull(result.ProcessedAt);
    }

    [Fact]
    public async Task GetBookingByIdAsync_AfterReject_ShouldReturnRejectedStatus()
    {
        // Arrange
        Setup();
        var createdEvent = CreateTestEvent();
        var createdBooking = await _bookingService.CreateBookingAsync(createdEvent.Id);

        // Act
        await _bookingService.RejectBookingAsync(createdBooking!.Id);
        var result = await _bookingService.GetBookingByIdAsync(createdBooking.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(BookingStatus.Rejected, result.Status);
        Assert.NotNull(result.ProcessedAt);
    }

    [Fact]
    public async Task CreateBookingAsync_WithNonExistingEvent_ShouldReturnNull()
    {
        // Arrange
        Setup();

        // Act
        var result = await _bookingService.CreateBookingAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateBookingAsync_WithDeletedEvent_ShouldReturnNull()
    {
        // Arrange
        Setup();
        var createdEvent = CreateTestEvent();
        _eventService.DeleteEvent(createdEvent.Id);

        // Act
        var result = await _bookingService.CreateBookingAsync(createdEvent.Id);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBookingByIdAsync_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        Setup();

        // Act
        var result = await _bookingService.GetBookingByIdAsync(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task CreateBookingAsync_DecreasesAvailableSeats()
    {
        // Arrange
        Setup();
        var ev = CreateTestEvent(totalSeats: 5);

        // Act
        await _bookingService.CreateBookingAsync(ev.Id);

        // Assert
        var updated = _eventService.GetEventById(ev.Id)!;
        Assert.Equal(4, updated.AvailableSeats);
    }

    [Fact]
    public async Task CreateBookingAsync_UpToLimit_AllSucceed()
    {
        // Arrange
        Setup();
        var ev = CreateTestEvent(totalSeats: 3);

        // Act
        var b1 = await _bookingService.CreateBookingAsync(ev.Id);
        var b2 = await _bookingService.CreateBookingAsync(ev.Id);
        var b3 = await _bookingService.CreateBookingAsync(ev.Id);

        // Assert
        Assert.NotNull(b1);
        Assert.NotNull(b2);
        Assert.NotNull(b3);
        Assert.NotEqual(b1.Id, b2.Id);
        Assert.NotEqual(b2.Id, b3.Id);
        var updated = _eventService.GetEventById(ev.Id)!;
        Assert.Equal(0, updated.AvailableSeats);
    }

    [Fact]
    public async Task CreateBookingAsync_WhenNoSeats_ThrowsNoAvailableSeatsException()
    {
        // Arrange
        Setup();
        var ev = CreateTestEvent(totalSeats: 1);
        await _bookingService.CreateBookingAsync(ev.Id);

        // Act & Assert
        await Assert.ThrowsAsync<NoAvailableSeatsException>(
            () => _bookingService.CreateBookingAsync(ev.Id));
    }

    [Fact]
    public async Task CreateBookingAsync_AfterRejectReleaseSeats_CanBookAgain()
    {
        // Arrange
        Setup();
        var ev = CreateTestEvent(totalSeats: 1);
        var booking = await _bookingService.CreateBookingAsync(ev.Id);

        var storedBooking = _bookingStore.GetById(booking!.Id)!;
        storedBooking.Reject();
        _bookingStore.Update(storedBooking);
        var storedEvent = _eventService.GetEventById(ev.Id)!;
        storedEvent.ReleaseSeats();
        _eventStore.Update(storedEvent);

        // Act
        var newBooking = await _bookingService.CreateBookingAsync(ev.Id);

        // Assert
        Assert.NotNull(newBooking);
        Assert.Equal(BookingStatus.Pending, newBooking.Status);
    }

    [Fact]
    public async Task CreateBookingAsync_Concurrent_PreventsOverbooking()
    {
        // Arrange
        Setup();
        const int totalSeats = 5;
        const int totalRequests = 20;
        var ev = CreateTestEvent(totalSeats: totalSeats);

        // Act
        var tasks = Enumerable.Range(0, totalRequests)
            .Select(_ => Task.Run(async () =>
            {
                try
                {
                    return await _bookingService.CreateBookingAsync(ev.Id);
                }
                catch (NoAvailableSeatsException)
                {
                    return null;
                }
            }));

        var results = await Task.WhenAll(tasks);

        // Assert
        var successful = results.Where(r => r is not null).ToList();
        Assert.Equal(totalSeats, successful.Count);

        var updated = _eventService.GetEventById(ev.Id)!;
        Assert.Equal(0, updated.AvailableSeats);
    }

    [Fact]
    public async Task CreateBookingAsync_Concurrent_AllSuccessfulBookingsHaveUniqueIds()
    {
        // Arrange
        Setup();
        const int totalSeats = 10;
        var ev = CreateTestEvent(totalSeats: totalSeats);

        // Act
        var tasks = Enumerable.Range(0, totalSeats)
            .Select(_ => Task.Run(() => _bookingService.CreateBookingAsync(ev.Id)));

        var results = await Task.WhenAll(tasks);

        // Assert
        var ids = results.Select(r => r!.Id).ToList();
        Assert.Equal(totalSeats, ids.Distinct().Count());
    }
}
