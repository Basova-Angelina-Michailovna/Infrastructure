using System.Collections.Concurrent;
using RabbitMQ.Client;

namespace StretchRoom.Infrastructure.HealthChecks;

internal static class HealthChecksCollectorService
{
    public static readonly ConcurrentBag<IConnectionFactory> Rabbits = [];
    public static readonly ConcurrentBag<string> Psqls = [];
}