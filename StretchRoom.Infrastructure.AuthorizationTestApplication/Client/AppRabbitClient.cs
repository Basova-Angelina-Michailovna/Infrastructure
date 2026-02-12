using EBCEYS.RabbitMQ.Client;
using EBCEYS.RabbitMQ.Configuration;
using EBCEYS.RabbitMQ.Server.MappedService.Data;
using JetBrains.Annotations;
using Newtonsoft.Json;
using StretchRoom.Infrastructure.AuthorizationTestApplication.BoundedContext;
using StretchRoom.Infrastructure.RabbitMq;

namespace StretchRoom.Infrastructure.AuthorizationTestApplication.Client;

/// <summary>
///     The <see cref="AppRabbitClient" /> class.
/// </summary>
[PublicAPI]
public class AppRabbitClient(
    ILogger<RabbitMQClient> clientLogger,
    RabbitMQConfiguration config,
    JsonSerializerSettings? serializerSettings = null)
    : SrRabbitMqClient(clientLogger, config, serializerSettings), IAppRabbitClient
{
    /// <inheritdoc />
    public Task<string?> GetOkAsync(CancellationToken token = default)
    {
        return SendRequestAsync<string>(new RabbitMQRequestData
        {
            Method = RoutesDictionary.RabbitMqControllerV1.Methods.GetOk
        }, token: token);
    }

    /// <inheritdoc />
    public Task<GenerateTokenResponse?> GenerateTokenAsync(GenerateTokenRequest request,
        CancellationToken token = default)
    {
        return SendRequestAsync<GenerateTokenResponse?>(new RabbitMQRequestData
        {
            Method = RoutesDictionary.RabbitMqControllerV1.Methods.GetJson,
            Params = [request]
        }, token: token);
    }
}