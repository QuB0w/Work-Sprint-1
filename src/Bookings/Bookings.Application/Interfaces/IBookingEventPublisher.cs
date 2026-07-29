using EventApi.Contracts;

namespace Bookings.Application.Interfaces;

public interface IBookingEventPublisher
{
    Task PublishBookingConfirmedAsync(BookingConfirmedEvent message);
}
