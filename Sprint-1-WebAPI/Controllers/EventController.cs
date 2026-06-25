using Interfaces;
using Microsoft.AspNetCore.Mvc;
using Sprint_1_WebAPI.Models;

[ApiController]
[Route("events")]
public class EventController(IEventService _eventService) : ControllerBase
{
    [HttpGet]
    public ActionResult<PaginatedResult<Event>> GetAllEvents(
        [FromQuery] string? title = null,
        [FromQuery] DateTime? from = null,
        [FromQuery] DateTime? to = null,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1)
        {
            return BadRequest("Page must be greater than 0.");
        }

        if (pageSize < 1 || pageSize > 100)
        {
            return BadRequest("PageSize must be between 1 and 100.");
        }

        var result = _eventService.GetEventsFiltered(title, from, to, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public ActionResult<Event> GetEventById(Guid id)
    {
        var foundEvent = _eventService.GetEventById(id);
        if (foundEvent is null)
        {
            return NotFound($"Event with id {id} was not found.");
        }

        return Ok(foundEvent);
    }

    [HttpPost]
    public ActionResult<Event> CreateEvent([FromBody] CreateEventRequest newEvent)
    {
        if (newEvent.StartAt >= newEvent.EndAt)
        {
            return BadRequest("StartAt must be earlier than EndAt.");
        }

        var createdEvent = _eventService.CreateEvent(newEvent);
        return CreatedAtAction(nameof(GetEventById), new { id = createdEvent.Id }, createdEvent);
    }

    [HttpPut("{id:guid}")]
    public ActionResult<Event> UpdateEvent(Guid id, [FromBody] UpdateEventRequest updatedEvent)
    {
        if (updatedEvent.StartAt >= updatedEvent.EndAt)
        {
            return BadRequest("StartAt must be earlier than EndAt.");
        }

        var updated = _eventService.UpdateEvent(id, updatedEvent);
        if (updated is null)
        {
            return NotFound($"Event with id {id} was not found.");
        }

        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public ActionResult DeleteEvent(Guid id)
    {
        var deleted = _eventService.DeleteEvent(id);
        if (!deleted)
        {
            return NotFound($"Event with id {id} was not found.");
        }

        return NoContent();
    }
}