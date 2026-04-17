using Event;
using Interfaces;

public class EventService : IEventService
{
    public static List<Events> Events { get; set; } = [];
    public List<Events> GetAllEvent()
    {
        return Events;
    }

    public Events GetEventById(int id)
    {
        return Events[id];
    }

    public void CreateEvent(Events newEvent)
    {
        Events.Add(newEvent);
    }

    public void UpdateEvent(int id, Events updatedEvent)
    {
        Events[id] = updatedEvent;
    }

    public void DeleteEvent(int id)
    {
        Events.RemoveAt(id);
    }
}