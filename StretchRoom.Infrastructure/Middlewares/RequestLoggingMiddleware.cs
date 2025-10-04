using System.Diagnostics;
using System.Text;
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
    private readonly string[] _noLoggingPaths =
    [
        "/swagger/",
        "/metrics"
    ];
    /// <summary>
    ///     Invokes the context.
    /// </summary>
    /// <param name="context">The context.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        var endpoint = context.GetEndpoint();

        var noRequestLogging = endpoint?.Metadata.GetMetadata<NoRequestBodyLoggingAttribute>();
        var noResponseLogging = endpoint?.Metadata.GetMetadata<NoResponseBodyLoggingAttribute>();
        var stopwatch = Stopwatch.StartNew();
        await LogRequest(context, noRequestLogging);

        var originalBodyStream = context.Response.Body;
        using var responseBody = new MemoryStream();
        context.Response.Body = responseBody;

        try
        {
            await next(context);
        }
        finally
        {
            stopwatch.Stop();
            await LogResponse(context, responseBody, originalBodyStream, stopwatch.Elapsed, noResponseLogging);
        }
    }

    private async Task LogRequest(HttpContext context, NoRequestBodyLoggingAttribute? bodyLoggingAttribute)
    {
        context.Request.EnableBuffering();

        var request = context.Request;
        var shouldLog = ShouldLogRequestBody(context.Request, bodyLoggingAttribute);
        var requestBody = shouldLog ? await ReadRequestBody(request) : "NO-LOGGING";

        logger.LogInformation("REQUEST({trace}) ==> {Method} {Path} {QueryString} {RequestBody}",
            context.TraceIdentifier,
            request.Method,
            request.GetDisplayUrl(),
            request.QueryString,
            requestBody);

        request.Body.Position = 0;
    }

    private async Task LogResponse(HttpContext context, MemoryStream responseBody, Stream originalBodyStream,
        TimeSpan duration, NoResponseBodyLoggingAttribute? bodyLoggingAttribute)
    {
        var shouldLog = ShouldLogResponseBody(context.Response, bodyLoggingAttribute);
        responseBody.Seek(0, SeekOrigin.Begin);
        var responseText = shouldLog ? await new StreamReader(responseBody).ReadToEndAsync() : "NO-LOGGING";
        responseBody.Seek(0, SeekOrigin.Begin);

        logger.LogInformation("<== RESPONSE({trace}): {StatusCode} {Path} {ContentType} {ResponseBody} {duration} ms",
            context.TraceIdentifier,
            context.Response.StatusCode,
            context.Request.GetDisplayUrl(),
            context.Response.ContentType,
            responseText,
            duration.Milliseconds);

        await responseBody.CopyToAsync(originalBodyStream);
    }

    private bool ShouldLogResponseBody(HttpResponse response, NoResponseBodyLoggingAttribute? bodyLoggingAttribute)
    {
        var contentType = response.ContentType?.ToLower() ?? "";

        var path = response.HttpContext.Request.Path.HasValue ? response.HttpContext.Request.Path.Value : string.Empty;
        var isInNoLogsList = _noLoggingPaths.Aggregate(false, (current, noLoggingPath) => current | path.Contains(noLoggingPath));

        return !isInNoLogsList 
               && bodyLoggingAttribute is null
               && ShouldLog()
               && (contentType.Contains("text/") ||
                   contentType.Contains("application/json") ||
                   contentType.Contains("application/xml"));
    }

    private bool ShouldLogRequestBody(HttpRequest request, NoRequestBodyLoggingAttribute? bodyLoggingAttribute)
    {
        var contentType = request.ContentType?.ToLower() ?? "";
        
        var path = request.Path.HasValue ? request.Path.Value : string.Empty;
        var isInNoLogsList = _noLoggingPaths.Aggregate(false, (current, noLoggingPath) => current | path.Contains(noLoggingPath));

        return !isInNoLogsList 
               && bodyLoggingAttribute is null
               && ShouldLog()
               && (contentType.Contains("text/") ||
                   contentType.Contains("application/json") ||
                   contentType.Contains("application/xml"));
    }

    private bool ShouldLog()
    {
        return logger.IsEnabled(LogLevel.Debug);
    }

    private async Task<string> ReadRequestBody(HttpRequest request)
    {
        if (request.ContentLength is null or 0)
            return string.Empty;

        try
        {
            using var reader = new StreamReader(
                request.Body,
                Encoding.UTF8,
                false,
                1024,
                true);

            var body = await reader.ReadToEndAsync();
            request.Body.Position = 0;
            return body;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to read request body");
            return "[Error reading request body]";
        }
    }
}