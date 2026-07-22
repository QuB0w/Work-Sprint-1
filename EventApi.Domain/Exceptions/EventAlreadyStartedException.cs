namespace EventApi.Domain.Exceptions;

public class EventAlreadyStartedException : Exception
{
    public EventAlreadyStartedException()
        : base("Cannot book an event that has already started.")
    {
    }

    public EventAlreadyStartedException(string message)
        : base(message)
    {
    }
}
