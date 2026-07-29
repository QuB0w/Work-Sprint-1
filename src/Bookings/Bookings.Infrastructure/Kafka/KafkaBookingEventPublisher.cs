using System.Text.Json;
using Bookings.Application.Interfaces;
using Confluent.Kafka;
using EventApi.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Bookings.Infrastructure.Kafka;

public class KafkaBookingEventPublisher : IBookingEventPublisher, IDisposable
{
    private readonly IProducer<string, string> _producer;
    private readonly ILogger<KafkaBookingEventPublisher> _logger;

    public KafkaBookingEventPublisher(IConfiguration configuration, ILogger<KafkaBookingEventPublisher> logger)
    {
        _logger = logger;
        var config = new ProducerConfig
        {
            BootstrapServers = configuration["Kafka:BootstrapServers"] ?? "localhost:9092"
        };
        _producer = new ProducerBuilder<string, string>(config).Build();
    }

    public async Task PublishBookingConfirmedAsync(BookingConfirmedEvent message)
    {
        var json = JsonSerializer.Serialize(message);
        var kafkaMessage = new Message<string, string>
        {
            Key = message.EventId.ToString(),
            Value = json
        };

        var result = await _producer.ProduceAsync(Topics.BookingConfirmed, kafkaMessage);
        _logger.LogInformation(
            "Published BookingConfirmed to {Topic}, partition {Partition}, offset {Offset}.",
            result.Topic, result.Partition.Value, result.Offset.Value);
    }

    public void Dispose()
    {
        _producer.Flush(TimeSpan.FromSeconds(5));
        _producer.Dispose();
    }
}
