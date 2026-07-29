using System.Text.Json;
using Confluent.Kafka;
using EventApi.Contracts;
using Events.Application.Caching;
using Events.Application.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Events.Infrastructure.Kafka;

public class BookingConfirmedConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<BookingConfirmedConsumer> _logger;
    private readonly ConsumerConfig _consumerConfig;

    public BookingConfirmedConsumer(
        IServiceScopeFactory scopeFactory,
        IConfiguration configuration,
        ILogger<BookingConfirmedConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _consumerConfig = new ConsumerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092",
            GroupId = configuration["Kafka:ConsumerGroup"] ?? "events-service",
            AutoOffsetReset = AutoOffsetReset.Earliest,
            EnableAutoCommit = true
        };
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        return Task.Run(() => ConsumeLoop(stoppingToken), stoppingToken);
    }

    private void ConsumeLoop(CancellationToken stoppingToken)
    {
        using var consumer = new ConsumerBuilder<string, string>(_consumerConfig).Build();
        consumer.Subscribe(Topics.BookingConfirmed);

        _logger.LogInformation("Subscribed to topic '{Topic}'.", Topics.BookingConfirmed);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var result = consumer.Consume(stoppingToken);
                if (result?.Message?.Value is null) continue;

                var message = JsonSerializer.Deserialize<BookingConfirmedEvent>(result.Message.Value);
                if (message is null) continue;

                ProcessMessage(message).GetAwaiter().GetResult();
            }
            catch (OperationCanceledException)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing message from Kafka.");
            }
        }

        consumer.Close();
    }

    private async Task ProcessMessage(BookingConfirmedEvent message)
    {
        using var scope = _scopeFactory.CreateScope();
        var repository = scope.ServiceProvider.GetRequiredService<IEventRepository>();
        var cache = scope.ServiceProvider.GetRequiredService<ICacheService>();

        var eventEntity = await repository.GetByIdAsync(message.EventId);
        if (eventEntity is null)
        {
            _logger.LogWarning("Event {EventId} not found, skipping.", message.EventId);
            return;
        }

        if (!eventEntity.TryDecrementSeats(message.Seats))
        {
            _logger.LogWarning("No available seats for Event {EventId}, skipping.", message.EventId);
            return;
        }

        await repository.UpdateAsync(eventEntity);
        await cache.RemoveAsync(CacheKeys.Event(message.EventId));
        _logger.LogInformation(
            "Decremented {Seats} seat(s) for Event {EventId}. Remaining: {Available}.",
            message.Seats, message.EventId, eventEntity.AvailableSeats);
    }
}
