namespace Sprint_1_WebAPI.Models;

public class Booking
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public enum BookingStatus
{
    Pending,
    Confirmed,
    Rejected
}

public class BookingInfo
{
    public Guid Id { get; set; }
    public Guid EventId { get; set; }
    public BookingStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ProcessedAt { get; set; }
}

public class CreateBookingRequest
{
    public Guid EventId { get; set; }
}
