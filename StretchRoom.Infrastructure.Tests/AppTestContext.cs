using System.Diagnostics;
using StretchRoom.Infrastructure.Tests.AppInitializer;
using StretchRoom.Tests.Infrastructure;
using Testcontainers.PostgreSql;

namespace StretchRoom.Infrastructure.Tests;

[SetUpFixture]
[NonParallelizable]
internal class AppTestContext
{
    private const string DatabaseName = "StretchRoom";
    private const string DbUser = "admin";
    private const string DbPassword = "password";
    private TestAppInitializer _app;

    private PostgreSqlContainer _postgres;

    public static AppContext AppContext { get; private set; }

    [OneTimeSetUp]
    public async Task Setup()
    {
        Trace.Listeners.Add(new ConsoleTraceListener());

        _postgres = new PostgreSqlBuilder().WithDatabase(DatabaseName).WithUsername(DbUser).WithPassword(DbPassword)
            .Build();
        await _postgres.StartAsync();

        var appPort = PortSelector.GetPort(5053);
        var healthChecksPort = PortSelector.GetPort(8080);

        _app = new TestAppInitializer(_postgres.GetConnectionString(), appPort, healthChecksPort);

        AppContext = new AppContext(_app.Server.Services, await _app.CreateAppClient(_app.Server));
    }

    [OneTimeTearDown]
    public async Task TearDown()
    {
        await _app.DisposeAsync();
        await _postgres.StopAsync();
        await _postgres.DisposeAsync();
    }
}