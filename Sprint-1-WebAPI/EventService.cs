using System.Collections.Concurrent;
using Interfaces;
using Sprint_1_WebAPI.Models;

public class EventService : IEventService
{
    private static readonly ConcurrentDictionary<Guid, Event> Events = new();

    public IReadOnlyCollection<Event> GetAllEvents()
    {
        return Events.Values
            .OrderBy(item => item.StartAt)
            .ToList();
    }

    public PaginatedResult<Event> GetEventsFiltered(string? title = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 10)
    {
        var query = Events.Values.AsQueryable();

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
        Events.TryGetValue(id, out var foundEvent);
        return foundEvent;
    }

    public Event CreateEvent(CreateEventRequest newEvent)
    {
        var createdEvent = new Event
        {
            Id = Guid.NewGuid(),
            Title = newEvent.Title.Trim(),
            Description = newEvent.Description,
            StartAt = newEvent.StartAt,
            EndAt = newEvent.EndAt,
        };

        Events[createdEvent.Id] = createdEvent;
        return createdEvent;
    }

    public Event? UpdateEvent(Guid id, UpdateEventRequest updatedEvent)
    {
        if (!Events.TryGetValue(id, out var existingEvent))
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
        };

        Events[id] = newState;
        return newState;
    }

    public bool DeleteEvent(Guid id)
    {
        return Events.TryRemove(id, out _);
    }

    public void ClearAllEvents()
    {
        Events.Clear();
    }
}