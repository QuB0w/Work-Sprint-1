using Sprint_1_WebAPI.Models;

namespace Sprint_1_WebAPI.DataAccess;

public interface IBookingStore
{
    Booking Add(Booking booking);
    Booking? GetById(Guid id);
    IReadOnlyCollection<Booking> GetPendingBookings();
    Booking Update(Booking booking);
    void ClearAll();
}
