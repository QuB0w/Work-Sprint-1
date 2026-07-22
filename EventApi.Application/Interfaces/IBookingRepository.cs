using EventApi.Domain.Entities;

namespace EventApi.Application.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<Booking?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Guid>> GetPendingBookingIdsAsync(CancellationToken cancellationToken = default);
    Task<int> CountActiveByUserAsync(Guid userId, CancellationToken cancellationToken = default);
    Task<Booking> AddAsync(Booking booking, CancellationToken cancellationToken = default);
    Task<Booking> UpdateAsync(Booking booking, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
    Task ClearAllAsync(CancellationToken cancellationToken = default);
}
