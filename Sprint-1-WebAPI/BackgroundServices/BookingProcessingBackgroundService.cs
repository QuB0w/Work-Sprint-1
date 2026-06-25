using Sprint_1_WebAPI.DataAccess;
using Sprint_1_WebAPI.Models;

namespace Sprint_1_WebAPI.BackgroundServices;

public class BookingProcessingBackgroundService : BackgroundService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingProcessingBackgroundService> _logger;

    public BookingProcessingBackgroundService(
        IServiceProvider serviceProvider,
        ILogger<BookingProcessingBackgroundService> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Booking processing background service started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await ProcessPendingBookingsAsync(stoppingToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Booking processing background service is stopping.");
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing bookings.");
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }

        _logger.LogInformation("Booking processing background service stopped.");
    }

    private async Task ProcessPendingBookingsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var bookingStore = scope.ServiceProvider.GetRequiredService<IBookingStore>();

        var pendingBookings = bookingStore.GetPendingBookings();

        foreach (var booking in pendingBookings)
        {
            stoppingToken.ThrowIfCancellationRequested();

            await Task.Delay(TimeSpan.FromSeconds(2), stoppingToken);

            booking.Confirm();
            bookingStore.Update(booking);

            _logger.LogInformation(
                "Booking {BookingId} for event {EventId} has been confirmed.",
                booking.Id,
                booking.EventId);
        }
    }
}
