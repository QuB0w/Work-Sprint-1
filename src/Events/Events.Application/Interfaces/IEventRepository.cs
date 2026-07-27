using Events.Domain.Entities;

namespace Events.Application.Interfaces;

public interface IEventRepository
{
    Task<Event?> GetByIdAsync(Guid id);
    Task<List<Event>> GetAllAsync();
    Task<Event> AddAsync(Event entity);
    Task UpdateAsync(Event entity);
    Task<bool> DeleteAsync(Guid id);
}
