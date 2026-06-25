using Sprint_1_WebAPI.Models;

namespace Interfaces;

public interface IBookingService
{
    Task<BookingInfo?> CreateBookingAsync(Guid eventId);
    Task<BookingInfo?> GetBookingByIdAsync(Guid bookingId);
    Task ConfirmBookingAsync(Guid bookingId);
    Task RejectBookingAsync(Guid bookingId);
    void ClearAllBookings();
}
