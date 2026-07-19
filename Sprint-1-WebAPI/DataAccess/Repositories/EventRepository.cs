using Microsoft.EntityFrameworkCore;
using Sprint_1_WebAPI.Models;

namespace Sprint_1_WebAPI.DataAccess.Repositories;

public sealed class EventRepository : IEventRepository
{
    private readonly AppDbContext _context;

    public EventRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyCollection<Event>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .AsNoTracking()
            .OrderBy(eventItem => eventItem.StartAt)
            .ToListAsync(cancellationToken);
    }

    public async Task<PaginatedResult<Event>> GetFilteredAsync(
        string? title,
        DateTime? from,
        DateTime? to,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        IQueryable<Event> query = _context.Events.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(title))
        {
            var normalizedTitle = title.ToLower();
            query = query.Where(eventItem => eventItem.Title.ToLower().Contains(normalizedTitle));
        }

        if (from.HasValue)
        {
            query = query.Where(eventItem => eventItem.StartAt >= from.Value);
        }

        if (to.HasValue)
        {
            query = query.Where(eventItem => eventItem.EndAt <= to.Value);
        }

        var totalCount = await query.CountAsync(cancellationToken);
        var items = await query
            .OrderBy(eventItem => eventItem.StartAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedResult<Event>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<Event?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events.FindAsync([id], cancellationToken);
    }

    public async Task<Event?> GetByIdAsNoTrackingAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Events
            .AsNoTracking()
            .FirstOrDefaultAsync(eventItem => eventItem.Id == id, cancellationToken);
    }

    public async Task<Event> AddAsync(Event eventItem, CancellationToken cancellationToken = default)
    {
        _context.Events.Add(eventItem);
        await _context.SaveChangesAsync(cancellationToken);
        return eventItem;
    }

    public async Task<Event> UpdateAsync(Event eventItem, CancellationToken cancellationToken = default)
    {
        _context.Events.Update(eventItem);
        await _context.SaveChangesAsync(cancellationToken);
        return eventItem;
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var eventItem = await GetByIdAsync(id, cancellationToken);
        if (eventItem is null)
        {
            return false;
        }

        _context.Events.Remove(eventItem);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task ClearAllAsync(CancellationToken cancellationToken = default)
    {
        _context.Events.RemoveRange(_context.Events);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
