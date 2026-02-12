using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace StretchRoom.Infrastructure.HealthChecks;

internal static class HealthchecksRegistrationExtensions
{
    private const string PsqlHealthNamePrefix = "psql";
    private const string RabbitHealthNamePrefix = "rabbit-mq";

    extension(IHealthChecksBuilder hcBuilder)
    {
        public void AddRabbitMqHealthChecks(HealthCheckConfiguration? configuration = null)
        {
            configuration ??= new HealthCheckConfiguration();
            var num = 1;
            foreach (var rabbit in HealthChecksCollectorService.Rabbits)
            {
                hcBuilder.AddRabbitMQ(_ => rabbit.CreateConnectionAsync(),
                    $"{RabbitHealthNamePrefix}-{configuration.NameFactory(num++)}",
                    configuration.FailureStatus,
                    configuration.Tags,
                    configuration.Timeout);
            }
        }

        public void AddNpgsqlHealthChecks(HealthCheckConfiguration? configuration = null)
        {
            configuration ??= new HealthCheckConfiguration();
            var num = 1;
            foreach (var psql in HealthChecksCollectorService.Psqls)
            {
                hcBuilder.AddNpgSql(psql,
                    name: $"{PsqlHealthNamePrefix}-{configuration.NameFactory(num++)}",
                    failureStatus: configuration.FailureStatus,
                    tags: configuration.Tags,
                    timeout: configuration.Timeout);
            }
        }
    }
}

/// <summary>
///     The <see cref="HealthCheckConfiguration" /> struct.
/// </summary>
[PublicAPI]
public record HealthCheckConfiguration
{
    /// <summary>
    ///     The health check name factory, where param is index of health check if there are more than one health check of same
    ///     type.
    /// </summary>
    public Func<int, string> NameFactory { get; init; } = ind => ind.ToString();

    /// <summary>
    ///     The tags.
    /// </summary>
    public IEnumerable<string>? Tags { get; init; }

    /// <summary>
    ///     The failure status.
    /// </summary>
    public HealthStatus FailureStatus { get; init; } = HealthStatus.Unhealthy;

    /// <summary>
    ///     The timeout.
    /// </summary>
    public TimeSpan? Timeout { get; init; }
}