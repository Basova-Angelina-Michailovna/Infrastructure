using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using StretchRoom.Infrastructure.Services.ExecutedServices;

namespace StretchRoom.Infrastructure;

/// <summary>
///     The
/// </summary>
/// <param name="builder"></param>
[PublicAPI]
public sealed class ConfiguredApp(IWebHostBuilder builder)
{
    /// <summary>
    ///     Builds and runs the application.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The <see cref="Task" /> that represents app running instance.</returns>
    public async Task BuildAndRunAsync(CancellationToken token = default)
    {
        var app = builder.Build();
        await app.Services.ExecuteAllBeforeHostingStarted(token);
        await app.RunAsync(token);
    }
}