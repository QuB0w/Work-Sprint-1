using Bookings.Application.Interfaces;
using Bookings.Domain.Entities;
using Bookings.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Bookings.Infrastructure.Repositories;

public class BookingRepository : IBookingRepository
{
    private readonly BookingsDbContext _context;

    public BookingRepository(BookingsDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(Guid id)
    {
        return await _context.Bookings.FindAsync(id);
    }

    public async Task<Booking> AddAsync(Booking booking)
    {
        _context.Bookings.Add(booking);
        await _context.SaveChangesAsync();
        return booking;
    }

    public async Task UpdateAsync(Booking booking)
    {
        _context.Bookings.Update(booking);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Guid>> GetPendingBookingIdsAsync()
    {
        return await _context.Bookings
            .Where(b => b.Status == BookingStatus.Pending)
            .Select(b => b.Id)
            .ToListAsync();
    }

    public async Task<int> CountActiveByUserAsync(Guid userId)
    {
        return await _context.Bookings
            .CountAsync(b => b.UserId == userId &&
                (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed));
    }
}
