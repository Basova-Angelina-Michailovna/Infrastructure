using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Serilog.Extensions.Logging;
using StretchRoom.Infrastructure.AuthorizationTestApplication.Client;
using StretchRoom.Infrastructure.HttpClient;
using StretchRoom.Infrastructure.Services.ExecutedServices;
using StretchRoom.Infrastructure.TestApplication;
using StretchRoom.Infrastructure.TestApplication.Client.Implementations;

namespace StretchRoom.Infrastructure.Tests.AppInitializer;

public class TestAppInitializer(string connectionString, int appPort, int healthChecksPort, AuthAppInitializer authServer)
    : WebApplicationFactory<Startup>
{
    private static System.Net.Http.HttpClient _authAppClient = null!;
    private static System.Net.Http.HttpClient _client = null!;

    protected override IHostBuilder CreateHostBuilder()
    {
        _authAppClient = authServer.CreateClient();
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(ConfigureConfiguration)
            .ConfigureLogging(opts =>
            {
                opts.ClearProviders();
                opts.SetMinimumLevel(LogLevel.Trace);
                opts.AddConsole();
            })
            .ConfigureWebHostDefaults(web =>
                web.UseStartup<OverloadStartup>().UseEnvironment("Development"));
    }

    private void ConfigureConfiguration(IConfigurationBuilder config)
    {
        config.Sources.Clear();
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "Urls", $"http://0.0.0.0:{appPort};http://0.0.0.0:{healthChecksPort}" },
            { "ConnectionStrings:TestApplication", connectionString },
            { "test-app:ServiceUrl", $"http://localhost:{appPort}" },
            { "auth-service:ServiceUrl", authServer.Server.BaseAddress.ToString() }
        });
    }

    public async Task<ITestApplicationClient> CreateAppClient(TestServer server)
    {
        _client = CreateClient();

        await server.Services.ExecuteAllBeforeHostingStarted();

        var flurlClientCache = Substitute.For<IFlurlClientCache>();
        flurlClientCache.GetOrAdd(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Action<IFlurlClientBuilder>>())
            .ReturnsForAnyArgs(call =>
            {
                var clientName = call.Args()[0] as string;
                var act = call.Arg<Action<IFlurlClientBuilder>>();
                var builder = new FlurlClientBuilder();
                act(builder);

                var isAuthClient = ClientBase.GetClientName(typeof(AuthAppClient));
                if (clientName == isAuthClient)
                {
                    var authClient = new FlurlClient(authServer.CreateClient());
                    authClient.Settings.Timeout = builder.Settings.Timeout;
                    authClient.Settings.AllowedHttpStatusRange = builder.Settings.AllowedHttpStatusRange;
                    authClient.Settings.JsonSerializer = builder.Settings.JsonSerializer;
                    authClient.Settings.HttpVersion = builder.Settings.HttpVersion;
                    return authClient;
                }
                var client = new FlurlClient(_client);
                client.Settings.Timeout = builder.Settings.Timeout;
                client.Settings.AllowedHttpStatusRange = builder.Settings.AllowedHttpStatusRange;
                client.Settings.JsonSerializer = builder.Settings.JsonSerializer;
                client.Settings.HttpVersion = builder.Settings.HttpVersion;

                return client;
            });

        return new TestApplicationClient(flurlClientCache, new SerilogLoggerFactory(),
            () => _client.BaseAddress!.ToString());
    }

    public class OverloadStartup(IConfiguration configuration) : Startup(configuration)
    {
        protected override void ServicesConfiguration(IServiceCollection services)
        {
            services.TryAddSingleton<IFlurlClientCache>(sp =>
            {
                var flurlClientCache = Substitute.For<IFlurlClientCache>();
                flurlClientCache.GetOrAdd(Arg.Any<string>(), Arg.Any<string>(), Arg.Any<Action<IFlurlClientBuilder>>())
                    .ReturnsForAnyArgs(call =>
                    {
                        var clientName = call.Args()[0] as string;
                        var act = call.Arg<Action<IFlurlClientBuilder>>();
                        var flurlClientBuilder = new FlurlClientBuilder();
                        act(flurlClientBuilder);

                        var isAuthClient = ClientBase.GetClientName(typeof(AuthAppClient));
                        if (clientName == isAuthClient)
                        {
                            var authClient = new FlurlClient(_authAppClient);
                            authClient.Settings.Timeout = flurlClientBuilder.Settings.Timeout;
                            authClient.Settings.AllowedHttpStatusRange =
                                flurlClientBuilder.Settings.AllowedHttpStatusRange;
                            authClient.Settings.JsonSerializer = flurlClientBuilder.Settings.JsonSerializer;
                            authClient.Settings.HttpVersion = flurlClientBuilder.Settings.HttpVersion;
                            return authClient;
                        }

                        var client = new FlurlClient();
                        client.Settings.Timeout = flurlClientBuilder.Settings.Timeout;
                        client.Settings.AllowedHttpStatusRange = flurlClientBuilder.Settings.AllowedHttpStatusRange;
                        client.Settings.JsonSerializer = flurlClientBuilder.Settings.JsonSerializer;
                        client.Settings.HttpVersion = flurlClientBuilder.Settings.HttpVersion;

                        return client;
                    });
                return flurlClientCache;
            });
            base.ServicesConfiguration(services);
            services.AddMvc().AddApplicationPart(typeof(Startup).Assembly);
        }
    }
}