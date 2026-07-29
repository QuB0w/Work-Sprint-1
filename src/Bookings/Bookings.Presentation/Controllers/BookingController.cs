using System.Security.Claims;
using Bookings.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Bookings.Presentation.Controllers;

[ApiController]
[Route("bookings")]
[Authorize]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost("/events/{eventId:guid}/book")]
    public async Task<IActionResult> CreateBooking(Guid eventId)
    {
        var userId = GetUserId();
        var booking = await _bookingService.CreateBookingAsync(eventId, userId);
        return Accepted(booking);
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        return booking is null ? NotFound() : Ok(booking);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        var userId = GetUserId();
        var role = User.FindFirstValue(ClaimTypes.Role) ?? "User";
        await _bookingService.CancelBookingAsync(id, userId, role);
        return NoContent();
    }

    private Guid GetUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.Parse(claim!);
    }
}
