using Microsoft.AspNetCore.Http;
using Prometheus;

namespace StretchRoom.Infrastructure.Middlewares;

internal sealed class RequestMetricsMiddleware(RequestDelegate next)
{
    public const string MetricsPath = "/metrics";

    public async Task Invoke(HttpContext httpContext)
    {
        var path = httpContext.Request.Path;
        var method = httpContext.Request.Method;

        var counter = Metrics.CreateCounter("prometheus_demo_request_total", "HTTP Requests Total",
            new CounterConfiguration
            {
                LabelNames = ["path", "method", "status"]
            });

        var statusCode = 200;

        try
        {
            await next.Invoke(httpContext);
        }
        catch (Exception)
        {
            statusCode = 500;
            counter.Labels(path, method, statusCode.ToString()).Inc();

            throw;
        }

        if (!path.Value?.EndsWith(MetricsPath) == true)
        {
            statusCode = httpContext.Response.StatusCode;
            counter.Labels(path, method, statusCode.ToString()).Inc();
        }
    }
}