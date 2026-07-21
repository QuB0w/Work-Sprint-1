using EventApi.Application.DTOs;

namespace EventApi.Application.Interfaces;

public interface IBookingService
{
    Task<BookingInfo?> CreateBookingAsync(Guid eventId);
    Task<BookingInfo?> GetBookingByIdAsync(Guid bookingId);
    Task ConfirmBookingAsync(Guid bookingId);
    Task RejectBookingAsync(Guid bookingId);
    Task ClearAllBookingsAsync();
}
