using Bookings.Application.Interfaces;
using EventApi.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Bookings.Application.Services;

public class BookingConfirmationService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingConfirmationService> _logger;

    public BookingConfirmationService(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingConfirmationService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);

            try
            {
                using var scope = _scopeFactory.CreateScope();
                var repository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
                var publisher = scope.ServiceProvider.GetRequiredService<IBookingEventPublisher>();

                var pendingIds = await repository.GetPendingBookingIdsAsync();

                foreach (var id in pendingIds)
                {
                    var booking = await repository.GetByIdAsync(id);
                    if (booking is null) continue;

                    booking.Confirm();
                    await repository.UpdateAsync(booking);

                    var message = new BookingConfirmedEvent(
                        BookingId: booking.Id,
                        EventId: booking.EventId,
                        UserId: booking.UserId,
                        Seats: 1,
                        ConfirmedAt: booking.ProcessedAt!.Value);

                    await publisher.PublishBookingConfirmedAsync(message);

                    _logger.LogInformation("Booking {BookingId} confirmed and event published.", booking.Id);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in booking confirmation loop.");
            }
        }
    }
}
