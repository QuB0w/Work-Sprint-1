using Event;
using Interfaces;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class EventController(IEventService _eventService) : ControllerBase
{
    [HttpGet]
    public ActionResult<List<Events>> GetAllEvents()
    {
        return _eventService.GetAllEvent();
    }

    [HttpGet("{id}")]
    public ActionResult<Events> GetEventById(int id)
    {
        var events = _eventService.GetAllEvent().ElementAtOrDefault(id);
        if (events == null)
        {
            return NotFound($"Event with id {id} was not found.");
        }

        return events;
    }

    [HttpPost]
    public ActionResult CreateEvent([FromBody] Events newEvent)
    {
        _eventService.CreateEvent(newEvent);
        return Created();
    }

    [HttpPut("{id}")]
    public ActionResult UpdateEvent(int id, [FromBody] Events updatedEvent)
    {
        var events = _eventService.GetAllEvent().ElementAtOrDefault(id);
        if (events == null)
        {
            return NotFound($"Event with id {id} was not found.");
        }

        _eventService.UpdateEvent(id, updatedEvent);
        return Ok();
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteEvent(int id)
    {
        var events = _eventService.GetAllEvent().ElementAtOrDefault(id);
        if (events == null)
        {
            return NotFound($"Event with id {id} was not found.");
        }

        _eventService.DeleteEvent(id);
        return Ok();
    }
}