using Events.Application.Caching;
using Events.Application.DTOs;
using Events.Application.Interfaces;
using Events.Domain.Entities;
using Microsoft.Extensions.Options;

namespace Events.Application.Services;

public class EventService : IEventService
{
    private const int TopEventsCount = 10;

    private readonly IEventRepository _repository;
    private readonly ICacheService _cache;
    private readonly CacheOptions _cacheOptions;

    public EventService(IEventRepository repository, ICacheService cache, IOptions<CacheOptions> cacheOptions)
    {
        _repository = repository;
        _cache = cache;
        _cacheOptions = cacheOptions.Value;
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
        var cacheKey = CacheKeys.Event(id);
        var cached = await _cache.GetAsync<EventDto>(cacheKey);
        if (cached is not null) return cached;

        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return null;

        var dto = MapToDto(entity);
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromSeconds(_cacheOptions.EventTtlSeconds));
        return dto;
    }

    public async Task<List<EventDto>> GetAllAsync()
    {
        var entities = await _repository.GetAllAsync();
        return entities.Select(MapToDto).ToList();
    }

    public async Task<List<EventDto>> GetTopPopularAsync()
    {
        var cached = await _cache.GetAsync<List<EventDto>>(CacheKeys.TopEvents);
        if (cached is not null) return cached;

        var entities = await _repository.GetTopPopularAsync(TopEventsCount);
        var dtos = entities.Select(MapToDto).ToList();
        await _cache.SetAsync(CacheKeys.TopEvents, dtos, TimeSpan.FromSeconds(_cacheOptions.TopEventsTtlSeconds));
        return dtos;
    }

    public async Task<EventDto?> UpdateAsync(Guid id, UpdateEventRequest request)
    {
        var entity = await _repository.GetByIdAsync(id);
        if (entity is null) return null;

        entity.Update(request.Title, request.Description,
            request.StartAt, request.EndAt, request.TotalSeats);
        await _repository.UpdateAsync(entity);
        await _cache.RemoveAsync(CacheKeys.Event(id));
        return MapToDto(entity);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var deleted = await _repository.DeleteAsync(id);
        if (deleted)
            await _cache.RemoveAsync(CacheKeys.Event(id));
        return deleted;
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
