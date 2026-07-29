using Events.Application.Interfaces;
using Events.Domain.Entities;
using Events.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Events.Infrastructure.Repositories;

public class EventRepository : IEventRepository
{
    private readonly EventsDbContext _context;

    public EventRepository(EventsDbContext context)
    {
        _context = context;
    }

    public async Task<Event?> GetByIdAsync(Guid id)
    {
        return await _context.Events.FindAsync(id);
    }

    public async Task<List<Event>> GetAllAsync()
    {
        return await _context.Events.AsNoTracking().ToListAsync();
    }

    public async Task<Event> AddAsync(Event entity)
    {
        _context.Events.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task UpdateAsync(Event entity)
    {
        _context.Events.Update(entity);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var entity = await _context.Events.FindAsync(id);
        if (entity is null) return false;
        _context.Events.Remove(entity);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Event>> GetTopPopularAsync(int count)
    {
        return await _context.Events
            .AsNoTracking()
            .OrderByDescending(e => (double)(e.TotalSeats - e.AvailableSeats) / e.TotalSeats)
            .Take(count)
            .ToListAsync();
    }
}
