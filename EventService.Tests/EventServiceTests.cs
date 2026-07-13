using Sprint_1_WebAPI.Models;
using Interfaces;
using Xunit;

namespace EventServiceUnitTests;

[Collection("Sequential")]
public class EventServiceTests
{
    private readonly IEventService _eventService;

    public EventServiceTests()
    {
        _eventService = new EventService();
    }

    private void Setup()
    {
        _eventService.ClearAllEvents();
    }

    [Fact]
    public void CreateEvent_ShouldReturnCreatedEvent()
    {
        // Arrange
        Setup();
        var request = new CreateEventRequest
        {
            Title = "Test Event",
            Description = "Test Description",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(2),
            TotalSeats = 1
        };

        // Act
        var result = _eventService.CreateEvent(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.Title.Trim(), result.Title);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal(request.StartAt, result.StartAt);
        Assert.Equal(request.EndAt, result.EndAt);
        Assert.NotEqual(Guid.Empty, result.Id);
    }

    [Fact]
    public void GetAllEvents_ShouldReturnAllEvents()
    {
        // Arrange
        Setup();
        var event1 = _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Event 1",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 1
        });

        var event2 = _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Event 2",
            StartAt = DateTime.Now.AddHours(2),
            EndAt = DateTime.Now.AddHours(3),
            TotalSeats = 1
        });

        // Act
        var result = _eventService.GetAllEvents();

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, e => e.Id == event1.Id);
        Assert.Contains(result, e => e.Id == event2.Id);
    }

    [Fact]
    public void GetEventById_WithValidId_ShouldReturnEvent()
    {
        // Arrange
        Setup();
        var createdEvent = _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Test Event",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 1
        });

        // Act
        var result = _eventService.GetEventById(createdEvent.Id);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdEvent.Id, result.Id);
        Assert.Equal(createdEvent.Title, result.Title);
    }

    [Fact]
    public void GetEventById_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        Setup();
        // Act
        var result = _eventService.GetEventById(Guid.NewGuid());

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void UpdateEvent_WithValidId_ShouldReturnUpdatedEvent()
    {
        // Arrange
        Setup();
        var createdEvent = _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Original Title",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 1
        });

        var updateRequest = new UpdateEventRequest
        {
            Title = "Updated Title",
            Description = "Updated Description",
            StartAt = DateTime.Now.AddHours(1),
            EndAt = DateTime.Now.AddHours(2)
        };

        // Act
        var result = _eventService.UpdateEvent(createdEvent.Id, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createdEvent.Id, result.Id);
        Assert.Equal(updateRequest.Title.Trim(), result.Title);
        Assert.Equal(updateRequest.Description, result.Description);
        Assert.Equal(updateRequest.StartAt, result.StartAt);
        Assert.Equal(updateRequest.EndAt, result.EndAt);
    }

    [Fact]
    public void UpdateEvent_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        Setup();
        var updateRequest = new UpdateEventRequest
        {
            Title = "Updated Title",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1)
        };

        // Act
        var result = _eventService.UpdateEvent(Guid.NewGuid(), updateRequest);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public void DeleteEvent_WithValidId_ShouldReturnTrue()
    {
        // Arrange
        Setup();
        var createdEvent = _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Test Event",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 1
        });

        // Act
        var result = _eventService.DeleteEvent(createdEvent.Id);

        // Assert
        Assert.True(result);
        Assert.Null(_eventService.GetEventById(createdEvent.Id));
    }

    [Fact]
    public void DeleteEvent_WithInvalidId_ShouldReturnFalse()
    {
        // Arrange
        Setup();
        // Act
        var result = _eventService.DeleteEvent(Guid.NewGuid());

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void GetEventsFiltered_ByTitle_ShouldReturnFilteredEvents()
    {
        // Arrange
        Setup();
        _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Conference 2024",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 1
        });

        _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Workshop",
            StartAt = DateTime.Now.AddHours(2),
            EndAt = DateTime.Now.AddHours(3),
            TotalSeats = 1
        });

        // Act
        var result = _eventService.GetEventsFiltered(title: "conference");

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("Conference 2024", result.Items.First().Title);
    }

    [Fact]
    public void GetEventsFiltered_ByDateRange_ShouldReturnFilteredEvents()
    {
        // Arrange
        Setup();
        var baseDate = DateTime.Now;
        _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Early Event",
            StartAt = baseDate.AddDays(-1),
            EndAt = baseDate.AddDays(-1).AddHours(1),
            TotalSeats = 1
        });

        _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Today Event",
            StartAt = baseDate,
            EndAt = baseDate.AddHours(1),
            TotalSeats = 1
        });

        _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Future Event",
            StartAt = baseDate.AddDays(1),
            EndAt = baseDate.AddDays(1).AddHours(1),
            TotalSeats = 1
        });

        // Act
        var result = _eventService.GetEventsFiltered(from: baseDate.AddHours(-1), to: baseDate.AddHours(2));

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("Today Event", result.Items.First().Title);
    }

    [Fact]
    public void GetEventsFiltered_WithPagination_ShouldReturnPagedResults()
    {
        // Arrange
        Setup();
        for (int i = 1; i <= 5; i++)
        {
            _eventService.CreateEvent(new CreateEventRequest
            {
                Title = $"Event {i}",
                StartAt = DateTime.Now.AddHours(i),
                EndAt = DateTime.Now.AddHours(i + 1),
                TotalSeats = 1
            });
        }

        // Act
        var result = _eventService.GetEventsFiltered(page: 2, pageSize: 2);

        // Assert
        Assert.Equal(2, result.Items.Count);
        Assert.Equal(5, result.TotalCount);
        Assert.Equal(2, result.Page);
        Assert.Equal(2, result.PageSize);
        Assert.Equal(3, result.TotalPages);
        Assert.True(result.HasPreviousPage);
        Assert.True(result.HasNextPage);
    }

    [Fact]
    public void GetEventsFiltered_WithCombinedFilters_ShouldReturnCorrectResults()
    {
        // Arrange
        Setup();
        var baseDate = DateTime.Now;
        _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Tech Conference",
            StartAt = baseDate,
            EndAt = baseDate.AddHours(2),
            TotalSeats = 1
        });

        _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Business Meeting",
            StartAt = baseDate,
            EndAt = baseDate.AddHours(1),
            TotalSeats = 1
        });

        _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Tech Workshop",
            StartAt = baseDate.AddDays(1),
            EndAt = baseDate.AddDays(1).AddHours(1),
            TotalSeats = 1
        });

        // Act
        var result = _eventService.GetEventsFiltered(title: "tech", from: baseDate.AddHours(-1), to: baseDate.AddHours(3));

        // Assert
        Assert.Single(result.Items);
        Assert.Equal("Tech Conference", result.Items.First().Title);
    }

    [Fact]
    public void CreateEvent_WithInvalidDates_ShouldStillCreateEvent()
    {
        // Arrange
        Setup();
        var request = new CreateEventRequest
        {
            Title = "Invalid Event",
            StartAt = DateTime.Now.AddHours(1),
            EndAt = DateTime.Now,
            TotalSeats = 1
        };

        // Act
        var result = _eventService.CreateEvent(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(request.StartAt, result.StartAt);
        Assert.Equal(request.EndAt, result.EndAt);
    }

    [Fact]
    public void UpdateEvent_WithInvalidDates_ShouldStillUpdateEvent()
    {
        // Arrange
        Setup();
        var createdEvent = _eventService.CreateEvent(new CreateEventRequest
        {
            Title = "Original Event",
            StartAt = DateTime.Now,
            EndAt = DateTime.Now.AddHours(1),
            TotalSeats = 1
        });

        var updateRequest = new UpdateEventRequest
        {
            Title = "Updated Event",
            StartAt = DateTime.Now.AddHours(1),
            EndAt = DateTime.Now
        };

        // Act
        var result = _eventService.UpdateEvent(createdEvent.Id, updateRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(updateRequest.StartAt, result.StartAt);
        Assert.Equal(updateRequest.EndAt, result.EndAt);
    }
}
