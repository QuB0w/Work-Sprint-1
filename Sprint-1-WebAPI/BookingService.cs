using Interfaces;
using Sprint_1_WebAPI.DataAccess;
using Sprint_1_WebAPI.Exceptions;
using Sprint_1_WebAPI.Models;

public class BookingService : IBookingService
{
    private readonly IEventService _eventService;
    private readonly IBookingStore _bookingStore;
    private readonly IEventStore _eventStore;
    private readonly object _bookingLock = new();

    public BookingService(IEventService eventService, IBookingStore bookingStore, IEventStore eventStore)
    {
        _eventService = eventService;
        _bookingStore = bookingStore;
        _eventStore = eventStore;
    }

    public Task<BookingInfo?> CreateBookingAsync(Guid eventId)
    {
        Booking booking;

        lock (_bookingLock)
        {
            var eventItem = _eventService.GetEventById(eventId);
            if (eventItem is null)
            {
                return Task.FromResult<BookingInfo?>(null);
            }

            if (!eventItem.TryReserveSeats())
            {
                throw new NoAvailableSeatsException();
            }

            _eventStore.Update(eventItem);

            booking = Booking.CreatePending(eventId);
            _bookingStore.Add(booking);
        }

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

        booking.Confirm();
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

        booking.Reject();
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
