using Serilog.Extensions.Logging;
using StretchRoom.Infrastructure.TestApplication;
using StretchRoom.Infrastructure.TestApplication.Client.Implementations;
using StretchRoom.Tests.Infrastructure.IntegrationTests.WebApplication;

namespace StretchRoom.Infrastructure.Tests.AppInitializer;

public class AppWebApplicationFactory(Action<IServiceCollection> beforeHostingStarted)
    : TestWebApplicationFactory<Startup>
{
    public override string ServiceId { get; } = "app";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(beforeHostingStarted);
        base.ConfigureWebHost(builder);
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
    }
}

public class AppTestClientContext(Action<IServiceCollection> beforeHostingStarted)
    : ClientTestContext<ITestApplicationClient, Startup>
{
    protected override TestWebApplicationFactory<Startup> CreateWebApplicationFactory()
    {
        return new AppWebApplicationFactory(beforeHostingStarted);
    }

    protected override ITestApplicationClient CreateServiceClient(string baseAddress)
    {
        var clientsCache = CreateFlurlCache();

        return new TestApplicationClient(clientsCache, new SerilogLoggerFactory(), () => baseAddress);
    }
}