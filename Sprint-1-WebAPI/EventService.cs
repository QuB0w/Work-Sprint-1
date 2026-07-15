using Interfaces;
using Microsoft.EntityFrameworkCore;
using Sprint_1_WebAPI.DataAccess;
using Sprint_1_WebAPI.Models;

public class EventService(AppDbContext context) : IEventService
{
    private readonly AppDbContext _context = context;

    public async Task<IReadOnlyCollection<Event>> GetAllEventsAsync()
    {
        return await _context.Events
            .AsNoTracking()
            .OrderBy(eventItem => eventItem.StartAt)
            .ToListAsync();
    }

    public async Task<PaginatedResult<Event>> GetEventsFilteredAsync(
        string? title = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 10)
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

        var totalCount = await query.CountAsync();
        var items = await query
            .OrderBy(eventItem => eventItem.StartAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PaginatedResult<Event>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public Task<Event?> GetEventByIdAsync(Guid id)
    {
        return _context.Events.AsNoTracking().FirstOrDefaultAsync(eventItem => eventItem.Id == id);
    }

    public async Task<Event> CreateEventAsync(CreateEventRequest newEvent)
    {
        var createdEvent = Event.Create(newEvent);

        _context.Events.Add(createdEvent);
        await _context.SaveChangesAsync();
        return createdEvent;
    }

    public async Task<Event?> UpdateEventAsync(Guid id, UpdateEventRequest updatedEvent)
    {
        var eventItem = await _context.Events.FindAsync(id);
        if (eventItem is null)
        {
            return null;
        }

        eventItem.Title = updatedEvent.Title.Trim();
        eventItem.Description = updatedEvent.Description;
        eventItem.StartAt = updatedEvent.StartAt;
        eventItem.EndAt = updatedEvent.EndAt;

        await _context.SaveChangesAsync();
        return eventItem;
    }

    public async Task<bool> DeleteEventAsync(Guid id)
    {
        var eventItem = await _context.Events.FindAsync(id);
        if (eventItem is null)
        {
            return false;
        }

        _context.Events.Remove(eventItem);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task ClearAllEventsAsync()
    {
        _context.Events.RemoveRange(_context.Events);
        await _context.SaveChangesAsync();
    }
}