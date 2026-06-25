using System.Collections.Concurrent;
using Sprint_1_WebAPI.Models;

namespace Sprint_1_WebAPI.DataAccess;

public class InMemoryBookingStore : IBookingStore
{
    private static readonly ConcurrentDictionary<Guid, Booking> Bookings = new();

    public Booking Add(Booking booking)
    {
        Bookings[booking.Id] = booking;
        return booking;
    }

    public Booking? GetById(Guid id)
    {
        Bookings.TryGetValue(id, out var booking);
        return booking;
    }

    public IReadOnlyCollection<Booking> GetPendingBookings()
    {
        return Bookings.Values
            .Where(b => b.Status == BookingStatus.Pending)
            .ToList();
    }

    public Booking Update(Booking booking)
    {
        Bookings[booking.Id] = booking;
        return booking;
    }

    public void ClearAll()
    {
        Bookings.Clear();
    }
}
