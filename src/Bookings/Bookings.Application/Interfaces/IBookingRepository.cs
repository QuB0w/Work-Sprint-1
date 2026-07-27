using Bookings.Domain.Entities;

namespace Bookings.Application.Interfaces;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(Guid id);
    Task<Booking> AddAsync(Booking booking);
    Task UpdateAsync(Booking booking);
    Task<List<Guid>> GetPendingBookingIdsAsync();
    Task<int> CountActiveByUserAsync(Guid userId);
}
