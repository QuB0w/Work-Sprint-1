using Sprint_1_WebAPI.Models;
using Xunit;

namespace EventServiceUnitTests;

public class BookingEntityTests
{
    [Fact]
    public void CreatePending_ShouldReturnBookingWithPendingStatus()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        // Act
        var booking = Booking.CreatePending(eventId);

        // Assert
        Assert.NotNull(booking);
        Assert.Equal(eventId, booking.EventId);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.NotEqual(Guid.Empty, booking.Id);
        Assert.True(booking.CreatedAt > DateTime.MinValue);
        Assert.Null(booking.ProcessedAt);
    }

    [Fact]
    public void Confirm_ShouldSetStatusToConfirmedAndProcessedAt()
    {
        // Arrange
        var booking = Booking.CreatePending(Guid.NewGuid());

        // Act
        booking.Confirm();

        // Assert
        Assert.Equal(BookingStatus.Confirmed, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
        Assert.True(booking.ProcessedAt >= booking.CreatedAt);
    }

    [Fact]
    public void Reject_ShouldSetStatusToRejectedAndProcessedAt()
    {
        // Arrange
        var booking = Booking.CreatePending(Guid.NewGuid());

        // Act
        booking.Reject();

        // Assert
        Assert.Equal(BookingStatus.Rejected, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
        Assert.True(booking.ProcessedAt >= booking.CreatedAt);
    }

    [Fact]
    public void CreatePending_MultipleCalls_ShouldProduceUniqueIds()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        // Act
        var booking1 = Booking.CreatePending(eventId);
        var booking2 = Booking.CreatePending(eventId);
        var booking3 = Booking.CreatePending(eventId);

        // Assert
        Assert.NotEqual(booking1.Id, booking2.Id);
        Assert.NotEqual(booking2.Id, booking3.Id);
        Assert.NotEqual(booking1.Id, booking3.Id);
    }
}
