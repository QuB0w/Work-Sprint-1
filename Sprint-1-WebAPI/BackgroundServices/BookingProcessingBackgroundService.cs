using Sprint_1_WebAPI.DataAccess;
using Sprint_1_WebAPI.Models;

namespace Sprint_1_WebAPI.BackgroundServices;

public class BookingProcessingBackgroundService : BackgroundService
{
    private static readonly TimeSpan PollingInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan ProcessingDelay = TimeSpan.FromSeconds(2);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<BookingProcessingBackgroundService> _logger;
    private readonly SemaphoreSlim _processingSemaphore = new(1, 1);

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

            try
            {
                await Task.Delay(PollingInterval, stoppingToken);
            }
            catch (OperationCanceledException)
            {
                break;
            }
        }

        _logger.LogInformation("Booking processing background service stopped.");
    }

    private async Task ProcessPendingBookingsAsync(CancellationToken stoppingToken)
    {
        using var scope = _serviceProvider.CreateScope();
        var bookingStore = scope.ServiceProvider.GetRequiredService<IBookingStore>();
        var eventStore = scope.ServiceProvider.GetRequiredService<IEventStore>();

        var pendingBookings = bookingStore.GetPendingBookings().ToList();

        if (pendingBookings.Count == 0)
        {
            return;
        }

        _logger.LogInformation(
            "Processing {Count} pending bookings in parallel.",
            pendingBookings.Count);

        var tasks = pendingBookings.Select(
            booking => ProcessBookingAsync(booking, bookingStore, eventStore, stoppingToken));

        await Task.WhenAll(tasks);
    }

    private async Task ProcessBookingAsync(
        Booking booking,
        IBookingStore bookingStore,
        IEventStore eventStore,
        CancellationToken stoppingToken)
    {
        try
        {
            await Task.Delay(ProcessingDelay, stoppingToken);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        await _processingSemaphore.WaitAsync(stoppingToken);
        try
        {
            var eventItem = eventStore.GetById(booking.EventId);
            if (eventItem is null)
            {
                booking.Reject();
                bookingStore.Update(booking);
                _logger.LogWarning(
                    "Booking {BookingId}: event {EventId} not found — booking rejected.",
                    booking.Id,
                    booking.EventId);
                return;
            }

            booking.Confirm();
            bookingStore.Update(booking);

            _logger.LogInformation(
                "Booking {BookingId} for event {EventId} confirmed.",
                booking.Id,
                booking.EventId);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Processing of booking {BookingId} was cancelled.", booking.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing booking {BookingId}.", booking.Id);
            try
            {
                var eventItem = eventStore.GetById(booking.EventId);
                booking.Reject();
                bookingStore.Update(booking);

                if (eventItem is not null)
                {
                    eventItem.ReleaseSeats();
                    eventStore.Update(eventItem);
                }
            }
            catch (Exception releaseEx)
            {
                _logger.LogError(releaseEx, "Failed to reject booking {BookingId} after error.", booking.Id);
            }
        }
        finally
        {
            _processingSemaphore.Release();
        }
    }
}
