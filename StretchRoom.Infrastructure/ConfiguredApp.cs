using JetBrains.Annotations;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using StretchRoom.Infrastructure.Services.ExecutedServices;

namespace StretchRoom.Infrastructure;

/// <summary>
///     The <see cref="ConfiguredApp"/> class.
/// </summary>
/// <param name="builder">The web host builder.</param>
[PublicAPI]
public sealed class ConfiguredApp(IWebHostBuilder builder) : IDisposable, IAsyncDisposable
{
    private IWebHost _app = null!;

    /// <summary>
    ///     The application service provider.
    /// </summary>
    public IServiceProvider ServiceProvider => _app.Services;

    /// <inheritdoc />
    public async ValueTask DisposeAsync()
    {
        await _app.StopAsync();
        Dispose();
    }

    /// <inheritdoc />
    public void Dispose()
    {
        _app.Dispose();
    }

    /// <summary>
    ///     Builds and runs the application.
    /// </summary>
    /// <param name="configureConf">The configures the app configuration.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The <see cref="Task" /> that represents app running instance.</returns>
    public async Task BuildAndRunAsync(Action<IConfigurationBuilder>? configureConf = null,
        CancellationToken token = default)
    {
        if (configureConf is not null) builder.ConfigureAppConfiguration(configureConf);

        _app = builder.Build();

        await _app.Services.ExecuteAllBeforeHostingStarted(token);
        
        await _app.RunAsync(token);
    }
}