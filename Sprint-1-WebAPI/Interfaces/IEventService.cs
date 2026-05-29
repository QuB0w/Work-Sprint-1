using Sprint_1_WebAPI.Models;

namespace Interfaces;

public interface IEventService
{
    IReadOnlyCollection<Event> GetAllEvents();
    PaginatedResult<Event> GetEventsFiltered(string? title = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 10);
    Event? GetEventById(Guid id);
    Event CreateEvent(CreateEventRequest newEvent);
    Event? UpdateEvent(Guid id, UpdateEventRequest updatedEvent);
    bool DeleteEvent(Guid id);
    void ClearAllEvents();
}