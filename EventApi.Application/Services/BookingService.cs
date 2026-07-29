using EventApi.Application.DTOs;
using EventApi.Application.Interfaces;
using EventApi.Domain.Entities;
using EventApi.Domain.Exceptions;

namespace EventApi.Application.Services;

public class BookingService : IBookingService
{
    private static readonly SemaphoreSlim BookingSemaphore = new(1, 1);

    private readonly IEventRepository _eventRepository;
    private readonly IBookingRepository _bookingRepository;

    public BookingService(IEventRepository eventRepository, IBookingRepository bookingRepository)
    {
        _eventRepository = eventRepository;
        _bookingRepository = bookingRepository;
    }

    public async Task<BookingInfo?> CreateBookingAsync(Guid eventId)
    {
        await BookingSemaphore.WaitAsync();
        try
        {
            var eventItem = await _eventRepository.GetByIdAsync(eventId);
            if (eventItem is null)
            {
                return null;
            }

            if (!eventItem.TryReserveSeats())
            {
                throw new NoAvailableSeatsException();
            }

            var booking = Booking.CreatePending(eventId);
            await _bookingRepository.AddAsync(booking);

            return MapToBookingInfo(booking);
        }
        finally
        {
            BookingSemaphore.Release();
        }
    }

    public async Task<BookingInfo?> GetBookingByIdAsync(Guid bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsNoTrackingAsync(bookingId);
        return booking is null ? null : MapToBookingInfo(booking);
    }

    public async Task ConfirmBookingAsync(Guid bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId)
            ?? throw new KeyNotFoundException($"Booking with id {bookingId} was not found.");

        booking.Confirm();
        await _bookingRepository.UpdateAsync(booking);
    }

    public async Task RejectBookingAsync(Guid bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId)
            ?? throw new KeyNotFoundException($"Booking with id {bookingId} was not found.");

        booking.Reject();
        await _bookingRepository.UpdateAsync(booking);
    }

    public Task ClearAllBookingsAsync()
    {
        return _bookingRepository.ClearAllAsync();
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
