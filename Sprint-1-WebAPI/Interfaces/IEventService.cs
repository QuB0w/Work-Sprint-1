using Sprint_1_WebAPI.Models;

namespace Interfaces;

public interface IEventService
{
    IReadOnlyCollection<Event> GetAllEvents();
    Event? GetEventById(Guid id);
    Event CreateEvent(CreateEventRequest newEvent);
    Event? UpdateEvent(Guid id, UpdateEventRequest updatedEvent);
    bool DeleteEvent(Guid id);
}