using Serilog.Extensions.Logging;
using StretchRoom.Infrastructure.AuthorizationTestApplication;
using StretchRoom.Infrastructure.AuthorizationTestApplication.Client;
using StretchRoom.Tests.Infrastructure.IntegrationTests.WebApplication;

namespace StretchRoom.Infrastructure.Tests.AppInitializer;

public class AuthAppWebApplicationFactory(Action<IServiceCollection> beforeHostingStarted)
    : TestWebApplicationFactory<AuthorizationStartup>
{
    public override string ServiceId => "auth-app";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureServices(beforeHostingStarted);
        base.ConfigureWebHost(builder);
    }

    protected override void ConfigureTestServices(IServiceCollection services)
    {
    }
}

public class AuthAppTestClientContext(Action<IServiceCollection> beforeHostingStarted)
    : ClientTestContext<IAuthAppClient, AuthorizationStartup>
{
    protected override TestWebApplicationFactory<AuthorizationStartup> CreateWebApplicationFactory()
    {
        return new AuthAppWebApplicationFactory(beforeHostingStarted);
    }

    protected override IAuthAppClient CreateServiceClient(string baseAddress)
    {
        var cache = CreateFlurlCache();
        return new AuthAppClient(cache, new SerilogLoggerFactory(), () => baseAddress);
    }
}