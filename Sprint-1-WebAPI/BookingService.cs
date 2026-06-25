using Interfaces;
using Sprint_1_WebAPI.DataAccess;
using Sprint_1_WebAPI.Models;

public class BookingService : IBookingService
{
    private readonly IEventService _eventService;
    private readonly IBookingStore _bookingStore;

    public BookingService(IEventService eventService, IBookingStore bookingStore)
    {
        _eventService = eventService;
        _bookingStore = bookingStore;
    }

    public Task<BookingInfo?> CreateBookingAsync(Guid eventId)
    {
        var eventItem = _eventService.GetEventById(eventId);
        if (eventItem is null)
        {
            return Task.FromResult<BookingInfo?>(null);
        }

        var booking = new Booking
        {
            Id = Guid.NewGuid(),
            EventId = eventId,
            Status = BookingStatus.Pending,
            CreatedAt = DateTime.UtcNow
        };

        _bookingStore.Add(booking);
        return Task.FromResult<BookingInfo?>(MapToBookingInfo(booking));
    }

    public Task<BookingInfo?> GetBookingByIdAsync(Guid bookingId)
    {
        var booking = _bookingStore.GetById(bookingId);
        if (booking is null)
        {
            return Task.FromResult<BookingInfo?>(null);
        }

        return Task.FromResult<BookingInfo?>(MapToBookingInfo(booking));
    }

    public Task ConfirmBookingAsync(Guid bookingId)
    {
        var booking = _bookingStore.GetById(bookingId);
        if (booking is null)
        {
            throw new KeyNotFoundException($"Booking with id {bookingId} was not found.");
        }

        booking.Status = BookingStatus.Confirmed;
        booking.ProcessedAt = DateTime.UtcNow;
        _bookingStore.Update(booking);

        return Task.CompletedTask;
    }

    public Task RejectBookingAsync(Guid bookingId)
    {
        var booking = _bookingStore.GetById(bookingId);
        if (booking is null)
        {
            throw new KeyNotFoundException($"Booking with id {bookingId} was not found.");
        }

        booking.Status = BookingStatus.Rejected;
        booking.ProcessedAt = DateTime.UtcNow;
        _bookingStore.Update(booking);

        return Task.CompletedTask;
    }

    public void ClearAllBookings()
    {
        _bookingStore.ClearAll();
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
