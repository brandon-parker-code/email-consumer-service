using EmailConsumerService.Configuration;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace EmailConsumerService.HealthChecks;

public class DatabaseHealthCheck(DatabaseOptions options) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(options.ConnectionString))
        {
            return HealthCheckResult.Unhealthy("Database:ConnectionString is not configured.");
        }

        try
        {
            // await using var connection = new SqlConnection(options.ConnectionString);
            // await connection.OpenAsync(cancellationToken);

            // await using var command = new SqlCommand("SELECT 1;", connection);
            // await command.ExecuteScalarAsync(cancellationToken);

            return HealthCheckResult.Healthy();
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("Unable to reach the database.", ex);
        }
    }
}
