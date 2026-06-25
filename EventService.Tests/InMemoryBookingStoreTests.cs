using Sprint_1_WebAPI.DataAccess;
using Sprint_1_WebAPI.Models;
using Xunit;

namespace EventServiceUnitTests;

[Collection("Sequential")]
public class InMemoryBookingStoreTests
{
    private readonly IBookingStore _bookingStore;

    public InMemoryBookingStoreTests()
    {
        _bookingStore = new InMemoryBookingStore();
    }

    private void Setup()
    {
        _bookingStore.ClearAll();
    }

    [Fact]
    public void Add_ShouldStoreBooking()
    {
        // Arrange
        Setup();
        var booking = Booking.CreatePending(Guid.NewGuid());

        // Act
        var result = _bookingStore.Add(booking);
        var found = _bookingStore.GetById(booking.Id);

        // Assert
        Assert.NotNull(found);
        Assert.Equal(result.Id, found.Id);
        Assert.Equal(BookingStatus.Pending, found.Status);
    }

    [Fact]
    public void GetById_WithExistingId_ShouldReturnBooking()
    {
        // Arrange
        Setup();
        var booking = Booking.CreatePending(Guid.NewGuid());
        _bookingStore.Add(booking);

        // Act
        var result = _bookingStore.GetById(booking.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(booking.Id, result.Id);
        Assert.Equal(booking.EventId, result.EventId);
    }

    [Fact]
    public void GetById_WithNonExistingId_ShouldReturnNull()
    {
        // Arrange
        Setup();

        // Act
        var result = _bookingStore.GetById(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void GetPendingBookings_ShouldReturnOnlyPendingBookings()
    {
        // Arrange
        Setup();
        var pending1 = Booking.CreatePending(Guid.NewGuid());
        var pending2 = Booking.CreatePending(Guid.NewGuid());
        var confirmed = Booking.CreatePending(Guid.NewGuid());
        confirmed.Confirm();

        _bookingStore.Add(pending1);
        _bookingStore.Add(pending2);
        _bookingStore.Add(confirmed);

        // Act
        var result = _bookingStore.GetPendingBookings();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, b => b.Id == pending1.Id);
        Assert.Contains(result, b => b.Id == pending2.Id);
        Assert.DoesNotContain(result, b => b.Id == confirmed.Id);
    }

    [Fact]
    public void Update_ShouldModifyBooking()
    {
        // Arrange
        Setup();
        var booking = Booking.CreatePending(Guid.NewGuid());
        _bookingStore.Add(booking);

        // Act
        booking.Confirm();
        _bookingStore.Update(booking);
        var result = _bookingStore.GetById(booking.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(BookingStatus.Confirmed, result.Status);
        Assert.NotNull(result.ProcessedAt);
    }

    [Fact]
    public void ClearAll_ShouldRemoveAllBookings()
    {
        // Arrange
        Setup();
        _bookingStore.Add(Booking.CreatePending(Guid.NewGuid()));
        _bookingStore.Add(Booking.CreatePending(Guid.NewGuid()));

        // Act
        _bookingStore.ClearAll();
        var result = _bookingStore.GetPendingBookings();

        // Assert
        Assert.Empty(result);
    }
}
