using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Extensions.Logging;

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
        var stopwatch = Stopwatch.StartNew();
        var request = context.Request;

        await LogRequestAsync(request, context.TraceIdentifier);

        try
        {
            await next.Invoke(context);
        }
        finally
        {
            var response = context.Response;

            stopwatch.Stop();
            await LogResponseAsync(response, context.TraceIdentifier, stopwatch.Elapsed);
        }
    }

    private async Task LogRequestAsync(HttpRequest request, string traceIdentifier)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            var body = !request.HasFormContentType
                ? await new StreamReader(request.Body).ReadToEndAsync()
                : "FORM-DATA";
            logger.LogDebug("Request ==> {path} | id: {correlationId} | body: {body}", request.Path, traceIdentifier,
                body);
        }
        else
        {
            logger.LogInformation("Request ==> {path} | id: {correlationId}", request.GetDisplayUrl(), traceIdentifier);
        }
    }

    private async Task LogResponseAsync(HttpResponse response, string traceIdentifier, TimeSpan duration)
    {
        if (logger.IsEnabled(LogLevel.Debug))
        {
            var containFormData = response.ContentType?.Contains("form") ?? false;
            var body = !containFormData
                ? await new StreamReader(response.Body).ReadToEndAsync()
                : "FORM-DATA";
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