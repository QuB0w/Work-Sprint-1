using Events.Application.Caching;
using Events.Application.DTOs;
using Events.Application.Interfaces;
using Events.Application.Services;
using Events.Domain.Entities;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Events.Tests;

public class EventServiceCachingTests
{
    private static EventService CreateSut(
        Mock<IEventRepository> repository,
        Mock<ICacheService> cache,
        CacheOptions? options = null)
    {
        return new EventService(
            repository.Object,
            cache.Object,
            Options.Create(options ?? new CacheOptions { EventTtlSeconds = 300, TopEventsTtlSeconds = 60 }));
    }

    [Fact]
    public async Task GetByIdAsync_CacheHit_DoesNotCallRepository()
    {
        var eventId = Guid.NewGuid();
        var cachedDto = new EventDto { Id = eventId, Title = "Cached", TotalSeats = 10, AvailableSeats = 5 };

        var repository = new Mock<IEventRepository>(MockBehavior.Strict);
        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.GetAsync<EventDto>(CacheKeys.Event(eventId), It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedDto);

        var sut = CreateSut(repository, cache);

        var result = await sut.GetByIdAsync(eventId);

        Assert.Equal(cachedDto, result);
        repository.Verify(r => r.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_CacheMiss_ReadsFromRepositoryAndPopulatesCache()
    {
        var eventId = Guid.NewGuid();
        var entity = Event.Create("Title", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 10);

        var repository = new Mock<IEventRepository>();
        repository.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync(entity);

        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.GetAsync<EventDto>(CacheKeys.Event(eventId), It.IsAny<CancellationToken>()))
            .ReturnsAsync((EventDto?)null);

        var sut = CreateSut(repository, cache);

        var result = await sut.GetByIdAsync(eventId);

        Assert.NotNull(result);
        repository.Verify(r => r.GetByIdAsync(eventId), Times.Once);
        cache.Verify(c => c.SetAsync(
            CacheKeys.Event(eventId),
            It.Is<EventDto>(d => d.Id == entity.Id),
            TimeSpan.FromSeconds(300),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateAsync_InvalidatesEventCache()
    {
        var eventId = Guid.NewGuid();
        var entity = Event.Create("Title", "Desc", DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 10);
        var request = new UpdateEventRequest
        {
            Title = "New Title",
            StartAt = DateTime.UtcNow,
            EndAt = DateTime.UtcNow.AddHours(2),
            TotalSeats = 20
        };

        var repository = new Mock<IEventRepository>();
        repository.Setup(r => r.GetByIdAsync(eventId)).ReturnsAsync(entity);

        var cache = new Mock<ICacheService>();

        var sut = CreateSut(repository, cache);

        var result = await sut.UpdateAsync(eventId, request);

        Assert.NotNull(result);
        repository.Verify(r => r.UpdateAsync(entity), Times.Once);
        cache.Verify(c => c.RemoveAsync(CacheKeys.Event(eventId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenDeleted_InvalidatesEventCache()
    {
        var eventId = Guid.NewGuid();

        var repository = new Mock<IEventRepository>();
        repository.Setup(r => r.DeleteAsync(eventId)).ReturnsAsync(true);

        var cache = new Mock<ICacheService>();

        var sut = CreateSut(repository, cache);

        var result = await sut.DeleteAsync(eventId);

        Assert.True(result);
        cache.Verify(c => c.RemoveAsync(CacheKeys.Event(eventId), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenNotFound_DoesNotTouchCache()
    {
        var eventId = Guid.NewGuid();

        var repository = new Mock<IEventRepository>();
        repository.Setup(r => r.DeleteAsync(eventId)).ReturnsAsync(false);

        var cache = new Mock<ICacheService>();

        var sut = CreateSut(repository, cache);

        var result = await sut.DeleteAsync(eventId);

        Assert.False(result);
        cache.Verify(c => c.RemoveAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetTopPopularAsync_CacheHit_DoesNotCallRepository()
    {
        var cachedList = new List<EventDto> { new() { Id = Guid.NewGuid(), Title = "Top" } };

        var repository = new Mock<IEventRepository>(MockBehavior.Strict);
        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.GetAsync<List<EventDto>>(CacheKeys.TopEvents, It.IsAny<CancellationToken>()))
            .ReturnsAsync(cachedList);

        var sut = CreateSut(repository, cache);

        var result = await sut.GetTopPopularAsync();

        Assert.Equal(cachedList, result);
        repository.Verify(r => r.GetTopPopularAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task GetTopPopularAsync_CacheMiss_ReadsFromRepositoryAndPopulatesCache()
    {
        var entities = new List<Event>
        {
            Event.Create("A", null, DateTime.UtcNow, DateTime.UtcNow.AddHours(1), 10)
        };

        var repository = new Mock<IEventRepository>();
        repository.Setup(r => r.GetTopPopularAsync(10)).ReturnsAsync(entities);

        var cache = new Mock<ICacheService>();
        cache.Setup(c => c.GetAsync<List<EventDto>>(CacheKeys.TopEvents, It.IsAny<CancellationToken>()))
            .ReturnsAsync((List<EventDto>?)null);

        var sut = CreateSut(repository, cache);

        var result = await sut.GetTopPopularAsync();

        Assert.Single(result);
        repository.Verify(r => r.GetTopPopularAsync(10), Times.Once);
        cache.Verify(c => c.SetAsync(
            CacheKeys.TopEvents,
            It.IsAny<List<EventDto>>(),
            TimeSpan.FromSeconds(60),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
