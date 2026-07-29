using Bookings.Application.DTOs;

namespace Bookings.Application.Interfaces;

public interface IBookingService
{
    Task<BookingDto> CreateBookingAsync(Guid eventId, Guid userId);
    Task<BookingDto?> GetBookingByIdAsync(Guid id);
    Task CancelBookingAsync(Guid bookingId, Guid userId, string role);
}
