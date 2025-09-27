using System.Diagnostics;
using System.Reflection;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;
using StretchRoom.Infrastructure.Attributes;

namespace StretchRoom.Infrastructure.Middlewares;

/// <summary>
///     The request logging middleware.
/// </summary>
/// <param name="next">The request delegate.</param>
/// <param name="logger">The logger.</param>
public class RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
{
    /// <summary>
    ///     Invokes the context.
    /// </summary>
    /// <param name="context">The context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var noRequestLogging = endpoint?.RequestDelegate?.Method.GetCustomAttribute<NoRequestBodyLoggingAttribute>();
        var noResponseLogging = endpoint?.RequestDelegate?.Method.GetCustomAttribute<NoResponseBodyLoggingAttribute>();
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        await LogRequestAsync(request, context.TraceIdentifier, noRequestLogging);

        try
        {
            await next.Invoke(context);
        }
        finally
        {
            var response = context.Response;

            stopwatch.Stop();
            await LogResponseAsync(response, context.TraceIdentifier, stopwatch.Elapsed, noResponseLogging);
        }
    }

    private async Task LogRequestAsync(HttpRequest request, string traceIdentifier,
        NoRequestBodyLoggingAttribute? noRequestLogging)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            var body = noRequestLogging is null
                ? await new StreamReader(request.Body).ReadToEndAsync()
                : "NO-LOGGING";
            logger.LogDebug("Request ==> {path} | id: {correlationId} | body: {body}", request.GetDisplayUrl(),
                traceIdentifier,
                body);
        }
        else
        {
            logger.LogInformation("Request ==> {path} | id: {correlationId}", request.GetDisplayUrl(), traceIdentifier);
        }
    }

    private async Task LogResponseAsync(HttpResponse response, string traceIdentifier, TimeSpan duration,
        NoResponseBodyLoggingAttribute? noResponseLogging)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            var body = noResponseLogging is null
                ? await new StreamReader(response.Body).ReadToEndAsync()
                : "NO-LOGGING";
            logger.LogDebug("Response <== {path} | id: {correlationId} | duration: {duration} | body: {body}",
                response.HttpContext.Request.GetDisplayUrl(), traceIdentifier, duration, body);
        }
        else
        {
            logger.LogInformation("Response <== {path} | id: {correlationId} | duration: {duration}",
                response.HttpContext.Request.GetDisplayUrl(), traceIdentifier, duration);
        }
    }
}