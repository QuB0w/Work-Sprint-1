using Sprint_1_WebAPI.Models;

namespace Interfaces;

public interface IEventService
{
    Task<IReadOnlyCollection<Event>> GetAllEventsAsync();
    Task<PaginatedResult<Event>> GetEventsFilteredAsync(string? title = null, DateTime? from = null, DateTime? to = null, int page = 1, int pageSize = 10);
    Task<Event?> GetEventByIdAsync(Guid id);
    Task<Event> CreateEventAsync(CreateEventRequest newEvent);
    Task<Event?> UpdateEventAsync(Guid id, UpdateEventRequest updatedEvent);
    Task<bool> DeleteEventAsync(Guid id);
    Task ClearAllEventsAsync();
}