using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;

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
    public Task BuildAndRunAsync(CancellationToken token = default)
    {
        return builder.Build().RunAsync(token);
    }
}