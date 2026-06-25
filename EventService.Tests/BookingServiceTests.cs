using Interfaces;
using Sprint_1_WebAPI.DataAccess;
using Sprint_1_WebAPI.Models;
using Xunit;

namespace EventServiceUnitTests;

[Collection("Sequential")]
public class BookingServiceTests
{
    private readonly IEventService _eventService;
    private readonly IBookingStore _bookingStore;
    private readonly IBookingService _bookingService;

    public BookingServiceTests()
    {
        _eventService = new EventService();
        _bookingStore = new InMemoryBookingStore();
        _bookingService = new BookingService(_eventService, _bookingStore);
    }

    private void Setup()
    {
        _eventService.ClearAllEvents();
        _bookingService.ClearAllBookings();
    }

    [Fact]
    public async Task CreateBookingAsync_WithExistingEvent_ShouldReturnPendingBooking()
    {
        // Arrange
        Setup();
        var createdEvent = _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Test Event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        });

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
        var createdEvent = _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Test Event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        });

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
        var createdEvent = _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Test Event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        });
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
        var createdEvent = _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Test Event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        });
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
        var createdEvent = _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Test Event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        });
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
        var createdEvent = _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Test Event",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(1)
        });
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
}
