using System.Text;
using Flurl.Http.Configuration;
using HealthChecks.UI.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StretchRoom.Infrastructure.Controllers;
using StretchRoom.Infrastructure.Exceptions;
using StretchRoom.Infrastructure.Models;

namespace StretchRoom.Infrastructure.HttpClient.ServiceClient;

/// <summary>
///     The <see cref="IServiceSystemClient" /> interface.
/// </summary>
public interface IServiceSystemClient
{
    /// <summary>
    ///     Pings the service.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>Completed task if pinged successfully.</returns>
    /// <exception cref="ApiException">Api exception if catched error while requesting.</exception>
    Task PingAsync(CancellationToken token = default);

    /// <summary>
    ///     Health checks the service.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>Completed task if service is healthy.</returns>
    /// <exception cref="ApiException">Api exception if catched error while requesting or service is unhealthy.</exception>
    Task HealthCheckAsync(CancellationToken token = default);

    /// <summary>
    ///     Health checks the service with information.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The health check result.</returns>
    /// <exception cref="ApiException"></exception>
    Task<UIHealthReport> HealthStatusCheckAsync(CancellationToken token = default);

    /// <summary>
    ///     Gets the service metrics.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The string with service metrics.</returns>
    Task<string> GetMetricsAsync(CancellationToken token = default);
}

/// <summary>
///     The <see cref="ServiceSystemClient" /> class.
/// </summary>
/// <param name="clientCache"></param>
/// <param name="loggerFactory"></param>
/// <param name="baseUrlResolver"></param>
/// <param name="tokenResolver"></param>
public class ServiceSystemClient(
    IOptions<ServiceApiInfo> apiInfo,
    IFlurlClientCache clientCache,
    ILoggerFactory loggerFactory,
    Func<string> baseUrlResolver,
    Func<Task<string>>? tokenResolver = null)
    : ClientBase(clientCache, loggerFactory, baseUrlResolver, tokenResolver), IServiceSystemClient
{
    /// <inheritdoc />
    public async Task PingAsync(CancellationToken token = default)
    {
        var response = await GetAsync<ProblemDetails>(
            url => url.AppendPathSegments(apiInfo.Value.BaseAddress, ServiceControllerRoutes.Controller,
                ServiceControllerRoutes.Methods.Ping),
            token: token);

        if (!response.IsSuccess)
        {
            ApiExceptionHelper.ThrowApiException(response);
        }
    }

    /// <inheritdoc />
    public async Task HealthCheckAsync(CancellationToken token = default)
    {
        var response = await GetAsync<ProblemDetails>(
            url => url.AppendPathSegments(apiInfo.Value.BaseAddress, ServiceControllerRoutes.Controller,
                ServiceControllerRoutes.Methods.Healthz),
            token: token);

        if (!response.IsSuccess)
        {
            ApiExceptionHelper.ThrowApiException(response);
        }
    }

    /// <inheritdoc />
    public async Task<UIHealthReport> HealthStatusCheckAsync(CancellationToken token = default)
    {
        var response = await GetJsonAsync<UIHealthReport, ProblemDetails>(
            url => url.AppendPathSegments(apiInfo.Value.BaseAddress, ServiceControllerRoutes.Controller,
                ServiceControllerRoutes.Methods.HealthzStatus),
            token: token);

        if (!response.IsSuccess || response.Result is null)
        {
            return ApiExceptionHelper.ThrowApiException(response);
        }

        return response.Result;
    }

    /// <inheritdoc />
    public async Task<string> GetMetricsAsync(CancellationToken token = default)
    {
        var response = await GetRawAsync<ProblemDetails>(
            url => url.AppendPathSegments(apiInfo.Value.BaseAddress, ServiceControllerRoutes.Controller,
                ServiceControllerRoutes.Methods.Metrics)
            , token: token);

        if (!response.IsSuccess || response.Result is null)
        {
            return ApiExceptionHelper.ThrowApiException<string>(response.Error, response.StatusCode);
        }

        return Encoding.UTF8.GetString(response.Result);
    }
}