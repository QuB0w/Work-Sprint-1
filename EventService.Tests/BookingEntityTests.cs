using EventApi.Domain.Entities;
using EventApi.Domain.Enums;
using Xunit;

namespace EventServiceUnitTests;

public class BookingEntityTests
{
    private static readonly Guid TestUserId = Guid.NewGuid();

    [Fact]
    public void CreatePending_ShouldReturnBookingWithPendingStatus()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        // Act
        var booking = Booking.CreatePending(eventId, TestUserId);

        // Assert
        Assert.NotNull(booking);
        Assert.Equal(eventId, booking.EventId);
        Assert.Equal(TestUserId, booking.UserId);
        Assert.Equal(BookingStatus.Pending, booking.Status);
        Assert.NotEqual(Guid.Empty, booking.Id);
        Assert.True(booking.CreatedAt > DateTime.MinValue);
        Assert.Null(booking.ProcessedAt);
    }

    [Fact]
    public void Confirm_ShouldSetStatusToConfirmedAndProcessedAt()
    {
        // Arrange
        var booking = Booking.CreatePending(Guid.NewGuid(), TestUserId);

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
        var booking = Booking.CreatePending(Guid.NewGuid(), TestUserId);

        // Act
        booking.Reject();

        // Assert
        Assert.Equal(BookingStatus.Rejected, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
        Assert.True(booking.ProcessedAt >= booking.CreatedAt);
    }

    [Fact]
    public void Cancel_ShouldSetStatusToCancelledAndProcessedAt()
    {
        // Arrange
        var booking = Booking.CreatePending(Guid.NewGuid(), TestUserId);

        // Act
        booking.Cancel();

        // Assert
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.NotNull(booking.ProcessedAt);
    }

    [Fact]
    public void Cancel_CalledTwice_ShouldBeIdempotent()
    {
        // Arrange
        var booking = Booking.CreatePending(Guid.NewGuid(), TestUserId);
        booking.Cancel();
        var firstProcessedAt = booking.ProcessedAt;

        // Act
        booking.Cancel();

        // Assert
        Assert.Equal(BookingStatus.Cancelled, booking.Status);
        Assert.Equal(firstProcessedAt, booking.ProcessedAt);
    }

    [Fact]
    public void CreatePending_MultipleCalls_ShouldProduceUniqueIds()
    {
        // Arrange
        var eventId = Guid.NewGuid();

        // Act
        var booking1 = Booking.CreatePending(eventId, TestUserId);
        var booking2 = Booking.CreatePending(eventId, TestUserId);
        var booking3 = Booking.CreatePending(eventId, TestUserId);

        // Assert
        Assert.NotEqual(booking1.Id, booking2.Id);
        Assert.NotEqual(booking2.Id, booking3.Id);
        Assert.NotEqual(booking1.Id, booking3.Id);
    }
}
