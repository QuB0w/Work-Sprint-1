namespace EventApi.Domain.Exceptions;

public class ActiveBookingLimitExceededException : Exception
{
    public const int MaxActiveBookings = 10;

    public ActiveBookingLimitExceededException()
        : base($"Active booking limit exceeded. A user cannot have more than {MaxActiveBookings} active bookings.")
    {
    }

    public ActiveBookingLimitExceededException(string message)
        : base(message)
    {
    }
}
