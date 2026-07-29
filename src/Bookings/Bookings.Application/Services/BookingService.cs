using Bookings.Application.DTOs;
using Bookings.Application.Interfaces;
using Bookings.Domain.Entities;

namespace Bookings.Application.Services;

public class BookingService : IBookingService
{
    private readonly IBookingRepository _repository;
    private const int MaxActiveBookings = 10;

    public BookingService(IBookingRepository repository)
    {
        _repository = repository;
    }

    public async Task<BookingDto> CreateBookingAsync(Guid eventId, Guid userId)
    {
        var activeCount = await _repository.CountActiveByUserAsync(userId);
        if (activeCount >= MaxActiveBookings)
            throw new InvalidOperationException($"User has reached the maximum of {MaxActiveBookings} active bookings.");

        var booking = Booking.CreatePending(eventId, userId);
        await _repository.AddAsync(booking);
        return MapToDto(booking);
    }

    public async Task<BookingDto?> GetBookingByIdAsync(Guid id)
    {
        var booking = await _repository.GetByIdAsync(id);
        return booking is null ? null : MapToDto(booking);
    }

    public async Task CancelBookingAsync(Guid bookingId, Guid userId, string role)
    {
        var booking = await _repository.GetByIdAsync(bookingId);
        if (booking is null)
            throw new KeyNotFoundException("Booking not found.");

        if (role != "Admin" && booking.UserId != userId)
            throw new UnauthorizedAccessException("You can only cancel your own bookings.");

        booking.Cancel();
        await _repository.UpdateAsync(booking);
    }

    private static BookingDto MapToDto(Booking b) => new()
    {
        Id = b.Id,
        EventId = b.EventId,
        UserId = b.UserId,
        Status = b.Status,
        CreatedAt = b.CreatedAt,
        ProcessedAt = b.ProcessedAt
    };
}
