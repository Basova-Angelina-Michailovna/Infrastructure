using Flurl.Http.Configuration;
using JetBrains.Annotations;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace StretchRoom.Infrastructure.HttpClient.ClientRegistration;

/// <summary>
///     The <see cref="ClientRegistrationExtensions" /> extensions.
/// </summary>
[PublicAPI]
public static class ClientRegistrationExtensions
{
    /// <summary>
    ///     Starts the registration process of <typeparamref name="TImplementation" /> in <paramref name="services" />.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <typeparam name="TInterface">The interface.</typeparam>
    /// <typeparam name="TImplementation">The implementation of <see cref="ClientBase" />.</typeparam>
    /// <returns>
    ///     The new instance of <see cref="ClientBaseRegistrationRegistrator{TInterface,TImplementation}" /> to configure
    ///     client.
    /// </returns>
    public static ClientBaseRegistrationRegistrator<TInterface, TImplementation> AddClient<TInterface, TImplementation>(
        this IServiceCollection services)
        where TImplementation : ClientBase, TInterface where TInterface : class
    {
        return new ClientBaseRegistrationRegistrator<TInterface, TImplementation>(services);
    }
}

/// <summary>
///     The <see cref="ClientBaseRegistrationRegistrator{TInterface,TImplementation}" /> class.<br />
///     Creates the new instance of <see cref="ClientBaseRegistrationRegistrator{TInterface,TImplementation}" />.
/// </summary>
/// <param name="services">The service collection.</param>
/// <typeparam name="TInterface">The <see cref="ClientBase" /> interface.</typeparam>
/// <typeparam name="TImplementation">The <see cref="ClientBase" /> implementation.</typeparam>
[PublicAPI]
public class ClientBaseRegistrationRegistrator<TInterface, TImplementation>(IServiceCollection services)
    where TImplementation : ClientBase, TInterface where TInterface : class
{
    private Func<string>? _baseUrlResolver;
    private Func<IServiceProvider, IFlurlClientCache>? _flurlClientCacheResolver;
    private Func<Task<string>>? _tokenResolver;

    /// <summary>
    ///     Configures the url for client from string.
    /// </summary>
    /// <param name="url">The url to server.</param>
    /// <returns></returns>
    public ClientBaseRegistrationRegistrator<TInterface, TImplementation> FromUrl(string url)
    {
        _baseUrlResolver = () => url;
        return this;
    }

    /// <summary>
    ///     Configures the server url for client from <see cref="IConfiguration" />.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    /// <param name="serviceName">The service name.</param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException">Throw if there's no section with <paramref name="serviceName" />.</exception>
    public ClientBaseRegistrationRegistrator<TInterface, TImplementation> FromConfiguration(
        IConfiguration configuration,
        string serviceName)
    {
        var clientConfiguration = configuration.GetSection(serviceName).Get<ClientConfiguration>()
                                  ?? throw new InvalidOperationException(
                                      $"Configuration section '{serviceName}' not found");
        _baseUrlResolver = () => clientConfiguration.ServiceUrl;
        return this;
    }

    /// <summary>
    ///     Adds the auth token resolver to <see cref="ClientBaseRegistrationRegistrator{TInterface,TImplementation}" />.<br />
    ///     Token will be placed in request headers with key <see cref="ClientBase.AuthorizationHeader" />.
    /// </summary>
    /// <param name="tokenResolver"></param>
    /// <returns></returns>
    public ClientBaseRegistrationRegistrator<TInterface, TImplementation> AddAuthTokenResolver(
        Func<Task<string>> tokenResolver)
    {
        _tokenResolver = tokenResolver;
        return this;
    }

    /// <summary>
    ///     Configures <see cref="IFlurlClientCache" />. If it wasn't configured the default implementation will be used.
    /// </summary>
    /// <param name="clientCacheResolver">The client cache resolver.</param>
    /// <returns></returns>
    public ClientBaseRegistrationRegistrator<TInterface, TImplementation> ConfigureClientCache(
        Func<IServiceProvider, IFlurlClientCache> clientCacheResolver)
    {
        _flurlClientCacheResolver = clientCacheResolver;
        return this;
    }

    /// <summary>
    ///     Registration of the <typeparamref name="TImplementation" /> in <see cref="IServiceCollection" />.
    /// </summary>
    /// <exception cref="InvalidOperationException"></exception>
    public void Register()
    {
        if (_baseUrlResolver is null) throw new InvalidOperationException("No service url resolver has been set");

        if (_flurlClientCacheResolver is not null)
            services.TryAddSingleton<IFlurlClientCache>(sp => _flurlClientCacheResolver(sp));
        else
            services.TryAddSingleton<IFlurlClientCache, FlurlClientCache>();

        if (_tokenResolver is not null)
        {
            services.AddSingleton<TInterface, TImplementation>(sp =>
                ActivatorUtilities.CreateInstance<TImplementation>(sp, _baseUrlResolver, _tokenResolver));
            return;
        }

        services.AddSingleton<TInterface, TImplementation>(sp =>
            ActivatorUtilities.CreateInstance<TImplementation>(sp, _baseUrlResolver));
    }
}

/// <summary>
///     The <see cref="ClientBase" /> configuration class.
/// </summary>
[PublicAPI]
public sealed class ClientConfiguration
{
    /// <summary>
    ///     The service url.
    /// </summary>
    public required string ServiceUrl { get; init; }
}