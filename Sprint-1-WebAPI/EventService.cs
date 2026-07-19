using Interfaces;
using Sprint_1_WebAPI.DataAccess.Repositories;
using Sprint_1_WebAPI.Models;

public class EventService(IEventRepository eventRepository) : IEventService
{
    private readonly IEventRepository _eventRepository = eventRepository;

    public Task<IReadOnlyCollection<Event>> GetAllEventsAsync()
    {
        return _eventRepository.GetAllAsync();
    }

    public Task<PaginatedResult<Event>> GetEventsFilteredAsync(
        string? title = null,
        DateTime? from = null,
        DateTime? to = null,
        int page = 1,
        int pageSize = 10)
    {
        return _eventRepository.GetFilteredAsync(title, from, to, page, pageSize);
    }

    public Task<Event?> GetEventByIdAsync(Guid id)
    {
        return _eventRepository.GetByIdAsNoTrackingAsync(id);
    }

    public async Task<Event> CreateEventAsync(CreateEventRequest newEvent)
    {
        var createdEvent = Event.Create(newEvent);
        return await _eventRepository.AddAsync(createdEvent);
    }

    public async Task<Event?> UpdateEventAsync(Guid id, UpdateEventRequest updatedEvent)
    {
        var eventItem = await _eventRepository.GetByIdAsync(id);
        if (eventItem is null)
        {
            return null;
        }

        eventItem.Title = updatedEvent.Title.Trim();
        eventItem.Description = updatedEvent.Description;
        eventItem.StartAt = updatedEvent.StartAt;
        eventItem.EndAt = updatedEvent.EndAt;

        return await _eventRepository.UpdateAsync(eventItem);
    }

    public Task<bool> DeleteEventAsync(Guid id)
    {
        return _eventRepository.DeleteAsync(id);
    }

    public Task ClearAllEventsAsync()
    {
        return _eventRepository.ClearAllAsync();
    }
}