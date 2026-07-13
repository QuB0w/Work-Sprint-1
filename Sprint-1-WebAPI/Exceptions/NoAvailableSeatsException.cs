namespace Sprint_1_WebAPI.Exceptions;

public class NoAvailableSeatsException : Exception
{
    public NoAvailableSeatsException()
        : base("No available seats for this event.")
    {
    }

    public NoAvailableSeatsException(string message)
        : base(message)
    {
    }
}
