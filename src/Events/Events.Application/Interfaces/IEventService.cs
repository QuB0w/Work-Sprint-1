using Events.Application.DTOs;

namespace Events.Application.Interfaces;

public interface IEventService
{
    Task<EventDto> CreateAsync(CreateEventRequest request);
    Task<EventDto?> GetByIdAsync(Guid id);
    Task<List<EventDto>> GetAllAsync();
    Task<EventDto?> UpdateAsync(Guid id, UpdateEventRequest request);
    Task<bool> DeleteAsync(Guid id);
}
