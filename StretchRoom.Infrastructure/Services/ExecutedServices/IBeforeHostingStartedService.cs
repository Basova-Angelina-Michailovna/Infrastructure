using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace StretchRoom.Infrastructure.Services.ExecutedServices;

/// <summary>
/// The <see cref="IBeforeHostingStartedService"/> interface.
/// </summary>
[PublicAPI]
public interface IBeforeHostingStartedService
{
    /// <summary>
    /// Executes the action.
    /// </summary>
    /// <returns></returns>
    void Execute();
}


/// <summary>
/// The <see cref="BeforeHostingStartedServiceExtensions"/> extensions class.
/// </summary>
[PublicAPI]
public static class BeforeHostingStartedServiceExtensions
{
    /// <summary>
    /// Register implementation of <see cref="IBeforeHostingStartedService"/> to <paramref name="services"/>.<br/>
    /// All <see cref="IBeforeHostingStartedService"/> will be executed after all services registration.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <typeparam name="TImplementation">The implementation of <see cref="IBeforeHostingStartedService"/>.</typeparam>
    /// <returns>The <paramref name="services"/>.</returns>
    public static IServiceCollection AddBeforeHostingStarted<TImplementation>(this IServiceCollection services)
        where TImplementation : class, IBeforeHostingStartedService
    {
        services.AddSingleton<IBeforeHostingStartedService, TImplementation>();
        return services;
    }

    /// <summary>
    /// Executes the all registered implementations of <see cref="IBeforeHostingStartedService"/>.
    /// </summary>
    /// <param name="services">The services.</param>
    internal static void ExecuteAllBeforeHostingStarted(this IServiceCollection services)
    {
        using var sp = services.BuildServiceProvider();
        var beforeHostingStartedServices = sp.GetServices<IBeforeHostingStartedService>();
        foreach (var beforeHostingStartedService in beforeHostingStartedServices)
        {
            beforeHostingStartedService.Execute();
        }
        services.RemoveAll<IBeforeHostingStartedService>();
    }
}