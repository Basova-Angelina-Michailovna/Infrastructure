using StretchRoom.Infrastructure.DatabaseRegistration;
using StretchRoom.Infrastructure.Extensions;
using StretchRoom.Infrastructure.Models;
using StretchRoom.Infrastructure.TestApplication.Commands;
using StretchRoom.Infrastructure.TestApplication.DaL;

namespace StretchRoom.Infrastructure.TestApplication;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplicationBase<Startup>.Create();
        await builder.Build(args).BuildAndRunAsync();
    }
}

public class Startup(IConfiguration configuration) : ExtraStartupBase(configuration)
{
    protected override int? HealthCheckPort { get; init; } = 8080;

    protected override ServiceApiInfo ServiceApiInfo { get; init; } = new("test-app", "/test-app", [
        "v1",
        "v2"
    ], "TestApplication");

    protected override void ServicesConfiguration(IServiceCollection services)
    {
        services.RegisterDbContext<DataModelContext>(opts =>
        {
            opts.MigrateDb = true;
            opts.ConnectionString = Configuration.GetConnectionString("TestApplication");
        });
        services.AddScopedCommand<AddEntityCommand, AddEntityCommandContext, AddEntityCommandResult>();
        services.AddScopedCommand<GetEntitiesCommand, GetEntitiesCommandContext, GetEntitiesCommandResult>();
    }

    protected override void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
    {
    }
}