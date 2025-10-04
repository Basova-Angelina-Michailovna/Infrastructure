using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace StretchRoom.Infrastructure.Services.ExecutedServices;

/// <summary>
///     The <see cref="IBeforeHostingStartedService" /> interface.
/// </summary>
[PublicAPI]
public interface IBeforeHostingStartedService
{
    /// <summary>
    ///     Executes the action.
    /// </summary>
    /// <returns></returns>
    Task ExecuteAsync(CancellationToken cancellationToken);
}

/// <summary>
///     The <see cref="BeforeHostingStartedServiceExtensions" /> extensions class.
/// </summary>
[PublicAPI]
public static class BeforeHostingStartedServiceExtensions
{
    /// <summary>
    ///     Register implementation of <see cref="IBeforeHostingStartedService" /> to <paramref name="services" />.<br />
    ///     All <see cref="IBeforeHostingStartedService" /> will be executed after all services registration.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <typeparam name="TImplementation">The implementation of <see cref="IBeforeHostingStartedService" />.</typeparam>
    /// <returns>The <paramref name="services" />.</returns>
    public static IServiceCollection AddBeforeHostingStarted<TImplementation>(this IServiceCollection services)
        where TImplementation : class, IBeforeHostingStartedService
    {
        services.AddSingleton<IBeforeHostingStartedService, TImplementation>();
        return services;
    }

    /// <summary>
    ///     Executes the all registered implementations of <see cref="IBeforeHostingStartedService" />.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="token">The cancellation token.</param>
    internal static async Task ExecuteAllBeforeHostingStarted(this IServiceProvider services,
        CancellationToken token = default)
    {
        var beforeHostingStartedServices = services.GetServices<IBeforeHostingStartedService>();
        foreach (var beforeHostingStartedService in beforeHostingStartedServices)
            await beforeHostingStartedService.ExecuteAsync(token);
    }
}