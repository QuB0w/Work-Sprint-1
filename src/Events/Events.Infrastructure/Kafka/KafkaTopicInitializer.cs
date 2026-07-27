using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EventApi.Contracts;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Events.Infrastructure.Kafka;

public class KafkaTopicInitializer : IHostedService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<KafkaTopicInitializer> _logger;

    public KafkaTopicInitializer(IConfiguration configuration, ILogger<KafkaTopicInitializer> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        var bootstrapServers = _configuration["Kafka:BootstrapServers"] ?? "localhost:9092";

        using var adminClient = new AdminClientBuilder(
            new AdminClientConfig { BootstrapServers = bootstrapServers }).Build();

        try
        {
            await adminClient.CreateTopicsAsync(new[]
            {
                new TopicSpecification
                {
                    Name = Topics.BookingConfirmed,
                    NumPartitions = 3,
                    ReplicationFactor = 1
                }
            });
            _logger.LogInformation("Topic '{Topic}' created.", Topics.BookingConfirmed);
        }
        catch (CreateTopicsException ex) when (ex.Results.Any(r => r.Error.Code == ErrorCode.TopicAlreadyExists))
        {
            _logger.LogInformation("Topic '{Topic}' already exists.", Topics.BookingConfirmed);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to create topic '{Topic}'. Continuing startup.", Topics.BookingConfirmed);
        }
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}
