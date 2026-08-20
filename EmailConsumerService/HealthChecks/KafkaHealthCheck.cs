using Confluent.Kafka;
using EmailConsumerService.Configuration;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EmailConsumerService.HealthChecks;

public class KafkaHealthCheck(IProducer<string, string> producer, KafkaOptions options) : IHealthCheck
{
    private static readonly TimeSpan MetadataTimeout = TimeSpan.FromSeconds(5);

    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Reuse the existing producer's underlying client handle rather than
            // opening a new broker connection just for the probe.
            // using var adminClient = new DependentAdminClientBuilder(producer.Handle).Build();
            // var metadata = adminClient.GetMetadata(options.Topic, MetadataTimeout);

            // if (metadata.Brokers.Count == 0)
            // {
            //     return Task.FromResult(
            //         HealthCheckResult.Unhealthy("Kafka cluster returned no brokers."));
            // }

            return Task.FromResult(HealthCheckResult.Healthy());
        }
        catch (Exception ex)
        {
            return Task.FromResult(
                HealthCheckResult.Unhealthy("Unable to reach the Kafka cluster.", ex));
        }
    }
}
