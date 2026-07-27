using Events.Application.DTOs;
using Events.Application.Interfaces;
using Events.Domain.Entities;

namespace Events.Application.Services;

public class EventService : IEventService
{
    private readonly IEventRepository _repository;

    public EventService(IEventRepository repository)
    {
        _repository = repository;
    }

    public async Task<EventDto> CreateAsync(CreateEventRequest request)
    {
        var entity = Event.Create(
            request.Title, request.Description,
            request.StartAt, request.EndAt, request.TotalSeats);
        await _repository.AddAsync(entity);
        return MapToDto(entity);
    }

    public async Task<EventDto?> GetByIdAsync(Guid id)
    {
        var entity = await _repository.GetByIdAsync(id);
        return entity is null ? null : MapToDto(entity);
    }

    public async Task<List<EventDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto).ToList();
    }

    public async Task<EventDto?> UpdateAsync(Guid id, UpdateEventRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return null;

        entity.Update(request.Title, request.Description,
            request.StartAt, request.EndAt, request.TotalSeats);
        await _repository.UpdateAsync(entity);
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _repository.DeleteAsync(id);
    }

    private static EventDto MapToDto(Event e) => new()
    {
        Id = e.Id,
        Title = e.Title,
        Description = e.Description,
        StartAt = e.StartAt,
        EndAt = e.EndAt,
        TotalSeats = e.TotalSeats,
        AvailableSeats = e.AvailableSeats
    };
}
