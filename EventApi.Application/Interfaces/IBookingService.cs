using EventApi.Application.DTOs;

namespace EventApi.Application.Interfaces;

public interface IBookingService
{
    Task<BookingInfo?> CreateBookingAsync(Guid eventId, Guid userId);
    Task<BookingInfo?> GetBookingByIdAsync(Guid bookingId);
    Task CancelBookingAsync(Guid bookingId, Guid userId, string userRole);
    Task ConfirmBookingAsync(Guid bookingId);
    Task RejectBookingAsync(Guid bookingId);
    Task ClearAllBookingsAsync();
}
