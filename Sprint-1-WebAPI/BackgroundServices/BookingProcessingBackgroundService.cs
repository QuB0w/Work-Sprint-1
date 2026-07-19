using Sprint_1_WebAPI.DataAccess.Repositories;
using Sprint_1_WebAPI.Models;

namespace Sprint_1_WebAPI.BackgroundServices;

public class BookingProcessingBackgroundService : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ProcessingDelay = TimeSpan.FromSeconds(2);

    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingProcessingBackgroundService> _logger;

    public BookingProcessingBackgroundService(
        IServiceScopeFactory scopeFactory,
        ILogger<BookingProcessingBackgroundService> logger)
    {
        _scopeFactory = scopeFactory;
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
                await Task.Delay(PollingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred while processing bookings.");
            }
        }

        _logger.LogInformation("Booking processing background service stopped.");
    }

    private async Task ProcessPendingBookingsAsync(CancellationToken stoppingToken)
    {
        await using var scope = _scopeFactory.CreateAsyncScope();
        var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
        var pendingBookingIds = await bookingRepository.GetPendingBookingIdsAsync(stoppingToken);

        await Task.WhenAll(pendingBookingIds.Select(id => ProcessBookingAsync(id, stoppingToken)));
    }

    private async Task ProcessBookingAsync(Guid bookingId, CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(ProcessingDelay, stoppingToken);

            await using var scope = _scopeFactory.CreateAsyncScope();
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            var booking = await bookingRepository.GetByIdAsync(bookingId, stoppingToken);
            if (booking is null || booking.Status != BookingStatus.Pending)
            {
                return;
            }

            var eventItem = await eventRepository.GetByIdAsync(booking.EventId, stoppingToken);
            if (eventItem is null)
            {
                booking.Reject();
                await bookingRepository.UpdateAsync(booking, stoppingToken);
                _logger.LogWarning(
                    "Booking {BookingId}: event {EventId} not found — booking rejected.",
                    booking.Id,
                    booking.EventId);
                return;
            }

            booking.Confirm();
            await bookingRepository.UpdateAsync(booking, stoppingToken);
            _logger.LogInformation("Booking {BookingId} for event {EventId} confirmed.", booking.Id, booking.EventId);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing booking {BookingId}.", bookingId);
            await RejectAndReleaseSeatAsync(bookingId);
        }
    }

    private async Task RejectAndReleaseSeatAsync(Guid bookingId)
    {
        try
        {
            await using var scope = _scopeFactory.CreateAsyncScope();
            var bookingRepository = scope.ServiceProvider.GetRequiredService<IBookingRepository>();
            var eventRepository = scope.ServiceProvider.GetRequiredService<IEventRepository>();

            var booking = await bookingRepository.GetByIdAsync(bookingId);
            if (booking is null || booking.Status != BookingStatus.Pending)
            {
                return;
            }

            var eventItem = await eventRepository.GetByIdAsync(booking.EventId);
            booking.Reject();
            if (eventItem is not null)
            {
                eventItem.ReleaseSeats();
            }

            await bookingRepository.UpdateAsync(booking);
        }
        catch (Exception releaseEx)
        {
            _logger.LogError(releaseEx, "Failed to reject booking {BookingId} after error.", bookingId);
        }
    }
}
