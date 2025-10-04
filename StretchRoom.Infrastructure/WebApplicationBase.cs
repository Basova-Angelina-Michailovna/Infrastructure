using JetBrains.Annotations;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace StretchRoom.Infrastructure;

/// <summary>
///     Initiates the new instance of <see cref="WebApplicationBase{TStartup}" />.
/// </summary>
/// <typeparam name="TStartup">The startup class based on <see cref="ExtraStartupBase" />.</typeparam>
[PublicAPI]
public class WebApplicationBase<TStartup> where TStartup : ExtraStartupBase
{
    /// <summary>
    ///     Builds the application.
    /// </summary>
    /// <param name="args">The arguments.</param>
    /// <returns></returns>
    public ConfiguredApp Build(string[] args)
    {
        var builder = WebHost.CreateDefaultBuilder(args);

        builder.UseStartup<TStartup>();

        return new ConfiguredApp(builder);
    }

    /// <summary>
    ///     Creates the new instance of <see cref="WebApplicationBase{TStartup}" />.
    /// </summary>
    /// <param name="args">The args.</param>
    /// <returns>The new instance of <see cref="WebApplicationBase{TStartup}" />.</returns>
    public static WebApplicationBase<TStartup> Create(params string[] args)
    {
        return new WebApplicationBase<TStartup>();
    }
}