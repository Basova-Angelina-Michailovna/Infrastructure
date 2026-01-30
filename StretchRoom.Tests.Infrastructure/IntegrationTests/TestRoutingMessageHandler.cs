using JetBrains.Annotations;
using StretchRoom.Tests.Infrastructure.IntegrationTests.WebApplication;

namespace StretchRoom.Tests.Infrastructure.IntegrationTests;

/// <summary>
///     The <see cref="TestRoutingMessageHandler" /> class.
/// </summary>
[PublicAPI]
public class TestRoutingMessageHandler : DelegatingHandler
{
    private readonly Dictionary<string, HostHandler> _hosts = new();

    /// <summary>
    ///     The route configuration.
    /// </summary>
    public static RoutingMessageHandlerConfiguration RouteConfiguration { get; } = new();

    /// <inheritdoc />
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var host = $"{request.RequestUri!.Scheme}://{request.RequestUri.Host}:{request.RequestUri.Port}/";

        if (_hosts.TryGetValue(host, out var handler))
        {
            return await handler.SendRequest(request, cancellationToken);
        }

        if (RouteConfiguration.Routes.TryGetValue(host, out var route))
        {
            handler = new HostHandler(route.Invoke());
            _hosts.Add(host, handler);

            return await handler.SendRequest(request, cancellationToken);
        }

        return await base.SendAsync(request, cancellationToken);
    }


    private class HostHandler(HttpMessageHandler handler) : DelegatingHandler(handler)
    {
        public Task<HttpResponseMessage> SendRequest(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            return SendAsync(request, cancellationToken);
        }
    }
}