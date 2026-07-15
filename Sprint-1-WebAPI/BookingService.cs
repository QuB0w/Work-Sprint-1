using Interfaces;
using Microsoft.EntityFrameworkCore;
using Sprint_1_WebAPI.DataAccess;
using Sprint_1_WebAPI.Exceptions;
using Sprint_1_WebAPI.Models;

public class BookingService(AppDbContext context) : IBookingService
{
    private static readonly SemaphoreSlim BookingSemaphore = new(1, 1);

    private readonly AppDbContext _context = context;

    public async Task<BookingInfo?> CreateBookingAsync(Guid eventId)
    {
        await BookingSemaphore.WaitAsync();
        try
        {
            var eventItem = await _context.Events.FirstOrDefaultAsync(eventItem => eventItem.Id == eventId);
            if (eventItem is null)
            {
                return null;
            }

            if (!eventItem.TryReserveSeats())
            {
                throw new NoAvailableSeatsException();
            }

            var booking = Booking.CreatePending(eventId);
            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return MapToBookingInfo(booking);
        }
        finally
        {
            BookingSemaphore.Release();
        }
    }

    public async Task<BookingInfo?> GetBookingByIdAsync(Guid bookingId)
    {
        var booking = await _context.Bookings
            .AsNoTracking()
            .FirstOrDefaultAsync(booking => booking.Id == bookingId);

        return booking is null ? null : MapToBookingInfo(booking);
    }

    public async Task ConfirmBookingAsync(Guid bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId)
            ?? throw new KeyNotFoundException($"Booking with id {bookingId} was not found.");

        booking.Confirm();
        await _context.SaveChangesAsync();
    }

    public async Task RejectBookingAsync(Guid bookingId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId)
            ?? throw new KeyNotFoundException($"Booking with id {bookingId} was not found.");

        booking.Reject();
        await _context.SaveChangesAsync();
    }

    public async Task ClearAllBookingsAsync()
    {
        _context.Bookings.RemoveRange(_context.Bookings);
        await _context.SaveChangesAsync();
    }

    private static BookingInfo MapToBookingInfo(Booking booking)
    {
        return new BookingInfo
        {
            Id = booking.Id,
            EventId = booking.EventId,
            Status = booking.Status,
            CreatedAt = booking.CreatedAt,
            ProcessedAt = booking.ProcessedAt
        };
    }
}
