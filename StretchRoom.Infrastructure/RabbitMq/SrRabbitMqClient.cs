using EBCEYS.RabbitMQ.Client;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace StretchRoom.Infrastructure.RabbitMq;

/// <summary>
///     The <see cref="SrRabbitMqClient" /> wrapper class for <see cref="RabbitMQClient" />.
/// </summary>
[PublicAPI]
public abstract class SrRabbitMqClient : IRabbitMQClient
{
    private readonly RabbitMQClient _client;

    /// <summary>
    ///     Initiates the new instance of <see cref="SrRabbitMqClient" />.
    /// </summary>
    /// <param name="clientLogger">The client logger.</param>
    /// <param name="config">The rabbitmq configuration.</param>
    /// <param name="serializerSettings">The JSON serializer settings.</param>
    protected SrRabbitMqClient(ILogger<RabbitMQClient> clientLogger, RabbitMQConfiguration config,
        JsonSerializerSettings? serializerSettings = null)
    {
        ConnectionString = config.Factory.Uri;
        _client = new RabbitMQClient(clientLogger, config, serializerSettings);
    }

    internal Uri ConnectionString { get; }

    /// <inheritdoc />
    public Task StartAsync(CancellationToken cancellationToken)
    {
        return _client.StartAsync(cancellationToken);
    }

    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken)
    {
        return _client.StopAsync(cancellationToken);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _client.Dispose();
    }

    /// <inheritdoc />
    public ValueTask DisposeAsync()
    {
        return _client.DisposeAsync();
    }

    /// <inheritdoc />
    public Task SendMessageAsync(RabbitMQRequestData data, bool mandatory = false,
        CancellationToken token = default)
    {
        return _client.SendMessageAsync(data, mandatory, token);
    }


    /// <inheritdoc />
    public Task<T?> SendRequestAsync<T>(RabbitMQRequestData data, bool mandatory = false,
        CancellationToken token = default)
    {
        return _client.SendRequestAsync<T>(data, mandatory, token);
    }
}