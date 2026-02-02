using System.Diagnostics;
using StretchRoom.Infrastructure.Middlewares;
using StretchRoom.Infrastructure.Scheduling;
using StretchRoom.Infrastructure.Tests.AppInitializer;
using StretchRoom.Infrastructure.Tests.Helpers;
using StretchRoom.Infrastructure.Tests.ScheduledTests.Helpers;
using StretchRoom.Tests.Infrastructure.Helpers;
using StretchRoom.Tests.Infrastructure.IntegrationTests;
using Testcontainers.PostgreSql;

namespace StretchRoom.Infrastructure.Tests;

[SetUpFixture]
[NonParallelizable]
internal class AppTestContext
{
    private const string DbUser = "admin";
    private const string DbPassword = "password";
    private static readonly string SolutionRelativePath = typeof(AppTestContext).Namespace!;

    private PostgreSqlContainer _postgres;

    public static AppTestClientContext AppContext { get; private set; }

    public static LogCatcherMiddleware<RequestLoggingMiddleware> AppLogCatcher { get; } = new();

    public static AuthAppTestClientContext AuthAppClientContext { get; private set; }


    [OneTimeSetUp]
    public async Task Setup()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        _postgres = new PostgreSqlBuilder().WithUsername(DbUser)
            .WithPassword(DbPassword)
            .Build();
        await _postgres.StartAsync();


        AuthAppClientContext = new AuthAppTestClientContext(sr =>
            sr.AddSingleton<Func<DelegatingHandler>>(() => new TestRoutingMessageHandler()));
        AuthAppClientContext.Initialize(conf =>
        {
            conf.UseProductionAppSettings = true;
            conf.SolutionRelativePath = SolutionRelativePath;
            conf.BuilderConfiguration = builder =>
            {
                builder.AddInMemoryConfig("JwtOptions:Issuer", "vitaliy");
                builder.AddInMemoryConfig("JwtOptions:Audience", "vitaliy");
                builder.AddInMemoryConfig("JwtOptions:Base64Key", new SrRandomizer().HexString(64));
            };
        });

        TestRoutingMessageHandler.RouteConfiguration.AddRoute(AuthAppClientContext);

        AppContext = new AppTestClientContext(sr =>
        {
            sr.AddSingleton<ILogger<RequestLoggingMiddleware>>(AppLogCatcher);
            sr.AddSchedulingServices(true, async factory =>
            {
                await factory.ScheduleJobAsync<ScheduledTestJob>(Guid.NewGuid().ToString(),
                    TimeSpan.FromMilliseconds(100));
            });
            sr.AddSingleton<Func<DelegatingHandler>>(() => new TestRoutingMessageHandler());
        });
        AppContext.Initialize(conf =>
        {
            conf.UseProductionAppSettings = true;
            conf.SolutionRelativePath = SolutionRelativePath;
            conf.BuilderConfiguration = builder =>
            {
                builder.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    { "ConnectionStrings:TestApplication", $"{_postgres.GetConnectionString()};Database=dev-db" },
                    { "auth-service:ServiceUrl", AuthAppClientContext.BaseAddress }
                });
            };
        });

        TestRoutingMessageHandler.RouteConfiguration.AddRoute(AppContext);
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        AuthAppClientContext.Teardown();
        AppContext.Teardown();

        await _postgres.StopAsync();
        await _postgres.DisposeAsync();
    }
}