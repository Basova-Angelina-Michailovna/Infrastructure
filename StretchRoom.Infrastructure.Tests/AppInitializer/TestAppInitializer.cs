using Flurl.Http;
using Flurl.Http.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Serilog.Extensions.Logging;
using StretchRoom.Infrastructure.Services.ExecutedServices;
using StretchRoom.Infrastructure.TestApplication;
using StretchRoom.Infrastructure.TestApplication.Client.Implementations;

namespace StretchRoom.Infrastructure.Tests.AppInitializer;

public class TestAppInitializer(string connectionString, int appPort, int healthChecksPort)
    : WebApplicationFactory<Startup>
{
    private System.Net.Http.HttpClient _client = null!;

    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder()
            .ConfigureAppConfiguration(ConfigureConfiguration)
            .ConfigureLogging(opts =>
            {
                opts.ClearProviders();
                opts.SetMinimumLevel(LogLevel.Trace);
                opts.AddConsole();
            })
            .ConfigureWebHostDefaults(web =>
                web.UseStartup<Startup>().UseEnvironment("Development"));
    }

    private void ConfigureConfiguration(IConfigurationBuilder config)
    {
        config.Sources.Clear();
        config.AddInMemoryCollection(new Dictionary<string, string?>
        {
            { "Urls", $"http://0.0.0.0:{appPort};http://0.0.0.0:{healthChecksPort}" },
            { "ConnectionStrings:TestApplication", connectionString },
            { "test-app:ServiceUrl", $"http://localhost:{appPort}" }
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
                var act = call.Arg<Action<IFlurlClientBuilder>>();
                var builder = new FlurlClientBuilder();
                act(builder);

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

    private class TestServerMessageHandler(TestServer testServer) : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            InnerHandler?.Dispose();
            InnerHandler = testServer.CreateHandler();
            return base.SendAsync(request, cancellationToken);
        }
    }
}