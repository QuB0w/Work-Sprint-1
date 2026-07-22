using EventApi.Domain.Enums;

namespace EventApi.Domain.Entities;

public class Booking
{
    private Booking()
    {
    }

    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public Event Event { get; set; } = null!;
    public Guid UserId { get; set; }
    public User User { get; set; } = null!;
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }

    public static Booking CreatePending(Guid eventId, Guid userId)
    {
        return new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            UserId = userId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Confirm()
    {
        Status = BookingStatus.Confirmed;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        Status = BookingStatus.Rejected;
        ProcessedAt = DateTime.UtcNow;
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Cancelled)
        {
            return;
        }

        Status = BookingStatus.Cancelled;
        ProcessedAt = DateTime.UtcNow;
    }
}
