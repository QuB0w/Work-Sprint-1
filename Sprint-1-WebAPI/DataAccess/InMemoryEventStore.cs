using System.Collections.Concurrent;
using Sprint_1_WebAPI.Models;

namespace Sprint_1_WebAPI.DataAccess;

public class InMemoryEventStore : IEventStore
{
    internal static readonly ConcurrentDictionary<Guid, Event> Events = new();

    public Event? GetById(Guid id)
    {
        Events.TryGetValue(id, out var eventItem);
        return eventItem;
    }

    public void Update(Event eventItem)
    {
        Events[eventItem.Id] = eventItem;
    }
}
