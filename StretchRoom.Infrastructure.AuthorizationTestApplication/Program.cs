using StretchRoom.Infrastructure.AuthorizationTestApplication.BoundedContext;
using StretchRoom.Infrastructure.Models;

namespace StretchRoom.Infrastructure.AuthorizationTestApplication;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplicationBase<AuthorizationStartup>.Create(args);
        await builder.Build(args).BuildAndRunAsync();
    }
}

public class AuthorizationStartup(IConfiguration configuration) : ExtraStartupBase(configuration)
{
    protected override ServiceApiInfo ServiceApiInfo { get; init; } =
        new(RoutesDictionary.ServiceName, RoutesDictionary.BasePath, ["v1"], "TestAuthService");

    protected override bool UseAuthentication { get; init; } = true;
    protected override bool ProxyToken { get; init; } = false;

    protected override void ServicesConfiguration(IServiceCollection services)
    {
        
    }

    protected override void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
    {
        
    }
}