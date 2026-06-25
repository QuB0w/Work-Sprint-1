using Interfaces;
using Microsoft.AspNetCore.Mvc;
using Sprint_1_WebAPI.Models;

[ApiController]
public class BookingController(IBookingService _bookingService) : ControllerBase
{
    [HttpPost("events/{id:guid}/book")]
    public async Task<ActionResult<BookingInfo>> CreateBooking(Guid id)
    {
        var booking = await _bookingService.CreateBookingAsync(id);
        if (booking is null)
        {
            return NotFound($"Event with id {id} was not found.");
        }

        var location = Url.Action(nameof(GetBookingById), new { id = booking.Id })
            ?? $"/bookings/{booking.Id}";

        return Accepted(location, booking);
    }

    [HttpGet("bookings/{id:guid}")]
    public async Task<ActionResult<BookingInfo>> GetBookingById(Guid id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        if (booking is null)
        {
            return NotFound($"Booking with id {id} was not found.");
        }

        return Ok(booking);
    }
}
