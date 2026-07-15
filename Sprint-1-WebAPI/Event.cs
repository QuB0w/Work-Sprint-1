using System.ComponentModel.DataAnnotations;

namespace Sprint_1_WebAPI.Models;

public class Event
{
    private Event()
    {
    }

    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public static Event Create(CreateEventRequest request)
    {
        if (request.TotalSeats <= 0)
        {
            throw new ArgumentException("TotalSeats must be greater than zero.");
        }

        return new Event
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Description = request.Description,
            StartAt = request.StartAt,
            EndAt = request.EndAt,
            TotalSeats = request.TotalSeats,
            AvailableSeats = request.TotalSeats
        };
    }

    public bool TryReserveSeats(int count = 1)
    {
        if (AvailableSeats < count)
        {
            return false;
        }

        AvailableSeats -= count;
        return true;
    }

    public void ReleaseSeats(int count = 1)
    {
        AvailableSeats = Math.Min(AvailableSeats + count, TotalSeats);
    }
}

public class CreateEventRequest
{
    [Required]
    [MinLength(1)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public DateTime StartAt { get; set; }

    [Required]
    public DateTime EndAt { get; set; }

    [Required]
    public int TotalSeats { get; set; }
}

public class UpdateEventRequest
{
    [Required]
    [MinLength(1)]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Required]
    public DateTime StartAt { get; set; }

    [Required]
    public DateTime EndAt { get; set; }
}