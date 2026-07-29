using EventApi.Domain.Entities;
using EventApi.Domain.Models;

namespace EventApi.Application.Interfaces;

public interface IEventRepository
{
    Task<IReadOnlyCollection<Event>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<PaginatedResult<Event>> GetFilteredAsync(
        string? title,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
    Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Event?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Event> AddAsync(Event eventItem, CancellationToken cancellationToken = default);
    Task<Event> UpdateAsync(Event eventItem, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task ClearAllAsync(CancellationToken cancellationToken = default);
}
