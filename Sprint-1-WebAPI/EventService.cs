using Interfaces;
using Sprint_1_WebAPI.DataAccess;
using Sprint_1_WebAPI.Models;

public class EventService : IEventService
{
    private static readonly Sprint_1_WebAPI.DataAccess.InMemoryEventStore Store = new();

    public IReadOnlyCollection<Event> GetAllEvents()
    {
        return InMemoryEventStore.Events.Values
            .OrderBy(item => item.StartAt)
            .ToList();
    }

    public PaginatedResult<Event> GetEventsFiltered(string? title = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 10)
    {
        var query = InMemoryEventStore.Events.Values.AsQueryable();

        if (!string.IsNullOrWhiteSpace(title))
        {
            query = query.Where(e => e.Title.Contains(title, StringComparison.OrdinalIgnoreCase));
        }

        if (from.HasValue)
        {
            query = query.Where(e => e.StartAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(e => e.EndAt <= to.Value);
        }

        var totalCount = query.Count();
        var items = query
            .OrderBy(e => e.StartAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        return new PaginatedResult<Event>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public Event? GetEventById(Guid id)
    {
        InMemoryEventStore.Events.TryGetValue(id, out var foundEvent);
        return foundEvent;
    }

    public Event CreateEvent(CreateEventRequest newEvent)
    {
        if (newEvent.TotalSeats <= 0)
        {
            throw new ArgumentException("TotalSeats must be greater than zero.");
        }

        var createdEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = newEvent.Title.Trim(),
            Description = newEvent.Description,
            StartAt = newEvent.StartAt,
            EndAt = newEvent.EndAt,
            TotalSeats = newEvent.TotalSeats,
            AvailableSeats = newEvent.TotalSeats,
        };

        InMemoryEventStore.Events[createdEvent.Id] = createdEvent;
        return createdEvent;
    }

    public Event? UpdateEvent(Guid id, UpdateEventRequest updatedEvent)
    {
        if (!InMemoryEventStore.Events.TryGetValue(id, out var existingEvent))
        {
            return null;
        }

        var newState = new Event
        {
            Id = existingEvent.Id,
            Title = updatedEvent.Title.Trim(),
            Description = updatedEvent.Description,
            StartAt = updatedEvent.StartAt,
            EndAt = updatedEvent.EndAt,
            TotalSeats = existingEvent.TotalSeats,
            AvailableSeats = existingEvent.AvailableSeats,
        };

        InMemoryEventStore.Events[id] = newState;
        return newState;
    }

    public bool DeleteEvent(Guid id)
    {
        return InMemoryEventStore.Events.TryRemove(id, out _);
    }

    public void ClearAllEvents()
    {
        InMemoryEventStore.Events.Clear();
    }
}