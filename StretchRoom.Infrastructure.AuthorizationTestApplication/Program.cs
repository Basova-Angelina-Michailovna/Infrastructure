using StretchRoom.Infrastructure.AuthorizationTestApplication.BoundedContext;
using StretchRoom.Infrastructure.AuthorizationTestApplication.SchedulingJobs;
using StretchRoom.Infrastructure.Models;
using StretchRoom.Infrastructure.Scheduling;

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
        services.AddSchedulingServices(true, async factory =>
        {
            await factory.ScheduleJobAsync<LogSomeJob>(Guid.NewGuid().ToString(), TimeSpan.FromSeconds(1));
        });
    }

    protected override void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
    {
        
    }
}