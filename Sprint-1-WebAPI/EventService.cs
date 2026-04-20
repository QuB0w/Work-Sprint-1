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
}