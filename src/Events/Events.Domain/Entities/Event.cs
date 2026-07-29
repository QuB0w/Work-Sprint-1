namespace Events.Domain.Entities;

public class Event
{
    public Guid Id { get; private set; }
    public string Title { get; private set; } = null!;
    public string? Description { get; private set; }
    public DateTime StartAt { get; private set; }
    public DateTime EndAt { get; private set; }
    public int TotalSeats { get; private set; }
    public int AvailableSeats { get; private set; }

    private Event() { }

    public static Event Create(string title, string? description, DateTime startAt, DateTime endAt, int totalSeats)
    {
        return new Event
        {
            Id = Guid.NewGuid(),
            Title = title,
            Description = description,
            StartAt = startAt,
            EndAt = endAt,
            TotalSeats = totalSeats,
            AvailableSeats = totalSeats
        };
    }

    public void Update(string title, string? description, DateTime startAt, DateTime endAt, int totalSeats)
    {
        var seatDiff = totalSeats - TotalSeats;
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
        TotalSeats = totalSeats;
        AvailableSeats = Math.Max(0, AvailableSeats + seatDiff);
    }

    public bool TryDecrementSeats(int count = 1)
    {
        if (AvailableSeats < count) return false;
        AvailableSeats -= count;
        return true;
    }

    public void IncrementSeats(int count = 1)
    {
        AvailableSeats = Math.Min(TotalSeats, AvailableSeats + count);
    }
}
