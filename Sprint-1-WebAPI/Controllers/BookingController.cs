using System.Security.Claims;
using EventApi.Application.DTOs;
using EventApi.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EventApi.Presentation.Controllers;

[ApiController]
[Authorize]
public class BookingController(IBookingService _bookingService) : ControllerBase
{
    [HttpPost("events/{id:guid}/book")]
    [ProducesResponseType(typeof(BookingInfo), StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<BookingInfo>> CreateBooking(Guid id)
    {
        var userId = GetUserId();
        var booking = await _bookingService.CreateBookingAsync(id, userId);
        if (booking is null)
        {
            return NotFound($"Event with id {id} was not found.");
        }

        var location = Url.Action(nameof(GetBookingById), new { id = booking.Id })
            ?? $"/bookings/{booking.Id}";

        return Accepted(location, booking);
    }

    [HttpGet("bookings/{id:guid}")]
    [ProducesResponseType(typeof(BookingInfo), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<BookingInfo>> GetBookingById(Guid id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking is null)
        {
            return NotFound($"Booking with id {id} was not found.");
        }

        return Ok(booking);
    }

    [HttpDelete("bookings/{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> CancelBooking(Guid id)
    {
        var userId = GetUserId();
        var userRole = User.FindFirstValue(ClaimTypes.Role) ?? "User";
        await _bookingService.CancelBookingAsync(id, userId, userRole);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(claim!);
    }
}
