using StretchRoom.Infrastructure.AuthorizationTestApplication.Client;
using StretchRoom.Infrastructure.DatabaseRegistration;
using StretchRoom.Infrastructure.Extensions;
using StretchRoom.Infrastructure.HttpClient.ClientRegistration;
using StretchRoom.Infrastructure.Models;
using StretchRoom.Infrastructure.TestApplication.BoundedContext;
using StretchRoom.Infrastructure.TestApplication.BoundedContext.Requests;
using StretchRoom.Infrastructure.TestApplication.Commands;
using StretchRoom.Infrastructure.TestApplication.DaL;
using StretchRoom.Infrastructure.TestApplication.Validators;

namespace StretchRoom.Infrastructure.TestApplication;

public class Program
{
    public static async Task Main(string[] args)
    {
        await TestAppInitiator.InitiateApp(args).BuildAndRunAsync();
    }
}

public static class TestAppInitiator
{
    public static ConfiguredApp InitiateApp(params string[] args)
    {
        var builder = WebApplicationBase<Startup>.Create(args);
        return builder.Build(args);
    }
}

public class Startup(IConfiguration configuration) : ExtraStartupBase(configuration)
{
    protected override int? HealthCheckPort { get; init; } = 8080;

    protected override ServiceApiInfo ServiceApiInfo { get; init; } = new("test-app", RoutesDictionary.BasePath, [
        "v1"
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
        services.AddScopedCommand<DeleteElementCommand, DeleteElementCommandContext, DeleteElementCommandResult>();
        services.AddScopedCommand<UpdateEntityCommand, UpdateEntityCommandContext, UpdateEntityCommandResult>();


        services.AddValidator<ChangeNameRequestValidator, ChangeNameRequest>();
        services.AddValidator<SomeBodyRequestValidator, SomeBodyRequest>();

        services.AddClient<IAuthAppClient, AuthAppClient>()
            .FromConfiguration(Configuration, AuthorizationTestApplication.BoundedContext.RoutesDictionary.ServiceName)
            .AddAuthTokenFromHttpContextResolver().Register();
    }

    protected override void ConfigureMiddlewares(IApplicationBuilder app, IHostEnvironment env)
    {
    }
}