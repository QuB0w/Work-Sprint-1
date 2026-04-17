namespace Event;

public class Events
{
    public required Guid Id {get; set;}
    public required string Title {get; set;}
    public string Description {get; set;}
    public required DateTime StartAt {get; set;}
    public required DateTime EndAt {get; set;}

    public Events(string title, string description, DateTime startAt, DateTime endAt)
    {
        if (string.IsNullOrEmpty(title))
            throw new ArgumentException("Title is required");
        if (string.IsNullOrEmpty(startAt.ToString()))
            throw new ArgumentException("StartAt is required");
        if (string.IsNullOrEmpty(endAt.ToString()))
            throw new ArgumentException("EndAt is required");

        Id = Guid.NewGuid();
        Title = title;
        Description = description;
        StartAt = startAt;
        EndAt = endAt;
    }
}