namespace EventApi.Contracts;

public sealed record BookingConfirmedEvent(
    Guid BookingId,
    Guid EventId,
    Guid UserId,
    int Seats,
    DateTime ConfirmedAt);
