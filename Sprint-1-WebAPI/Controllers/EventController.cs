using EventApi.Application.DTOs;
using EventApi.Application.Interfaces;
using EventApi.Domain.Entities;
using EventApi.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace EventApi.Presentation.Controllers;

[ApiController]
[Route("events")]
public class EventController(IEventService _eventService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<PaginatedResult<Event>>> GetAllEvents(
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

        var result = await _eventService.GetEventsFilteredAsync(title, from, to, page, pageSize);
        return Ok(result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Event>> GetEventById(Guid id)
    {
        var foundEvent = await _eventService.GetEventByIdAsync(id);
        if (foundEvent is null)
        {
            return NotFound($"Event with id {id} was not found.");
        }

        return Ok(foundEvent);
    }

    [HttpPost]
    public async Task<ActionResult<Event>> CreateEvent([FromBody] CreateEventRequest newEvent)
    {
        if (newEvent.StartAt >= newEvent.EndAt)
        {
            return BadRequest("StartAt must be earlier than EndAt.");
        }

        var createdEvent = await _eventService.CreateEventAsync(newEvent);
        return CreatedAtAction(nameof(GetEventById), new { id = createdEvent.Id }, createdEvent);
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<Event>> UpdateEvent(Guid id, [FromBody] UpdateEventRequest updatedEvent)
    {
        if (updatedEvent.StartAt >= updatedEvent.EndAt)
        {
            return BadRequest("StartAt must be earlier than EndAt.");
        }

        var updated = await _eventService.UpdateEventAsync(id, updatedEvent);
        if (updated is null)
        {
            return NotFound($"Event with id {id} was not found.");
        }

        return Ok(updated);
    }

    [HttpDelete("{id:guid}")]
    public async Task<ActionResult> DeleteEvent(Guid id)
    {
        var deleted = await _eventService.DeleteEventAsync(id);
        if (!deleted)
        {
            return NotFound($"Event with id {id} was not found.");
        }

        return NoContent();
    }
}