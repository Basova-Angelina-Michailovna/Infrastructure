using Microsoft.AspNetCore.Builder;
using StretchRoom.Infrastructure.Middlewares;

namespace StretchRoom.Infrastructure.Extensions;

/// <summary>
///     The <see cref="IApplicationBuilder" /> extensions.
/// </summary>
internal static class SrIApplicationBuilderExtensions
{
    /// <summary>
    ///     Adds the <see cref="RequestLoggingMiddleware" /> to <paramref name="app" />.
    /// </summary>
    /// <param name="app">The application builder.</param>
    public static void UseRequestLogging(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestLoggingMiddleware>();
    }

    /// <summary>
    ///     Adds the <see cref="ExceptionCatcherMiddleware" /> to <paramref name="app" />.
    /// </summary>
    /// <param name="app">The application builder.</param>
    public static void UseExceptionCatcher(this IApplicationBuilder app)
    {
        app.UseMiddleware<ExceptionCatcherMiddleware>();
    }

    /// <summary>
    ///     Adds the <see cref="RequestMetricsMiddleware" /> to <paramref name="app" />.
    /// </summary>
    /// <param name="app">The application builder.</param>
    public static void UseRequestMetrics(this IApplicationBuilder app)
    {
        app.UseMiddleware<RequestMetricsMiddleware>();
    }
}