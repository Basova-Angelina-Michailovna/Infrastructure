using AwesomeAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StretchRoom.Infrastructure.AuthorizationTestApplication.Client;
using StretchRoom.Infrastructure.Helpers;
using StretchRoom.Infrastructure.HttpClient.TokenManager;
using StretchRoom.Infrastructure.Interfaces;
using StretchRoom.Infrastructure.TestApplication.BoundedContext.Requests;
using StretchRoom.Infrastructure.TestApplication.Client.Implementations;
using StretchRoom.Infrastructure.TestApplication.Commands;
using StretchRoom.Infrastructure.TestApplication.DaL;

namespace StretchRoom.Infrastructure.Tests.DiTests;

public class DiCTests
{
    private IServiceProvider _context;

    [SetUp]
    public void SetUp()
    {
        _context = AppTestContext.AppContext.ServiceProvider;
    }

    [Test]
    public void When_AppIsRunning_With_TestApp_Result_HttpContextAccessorExists()
    {
        var accessor = _context.GetService<IHttpContextAccessor>();

        accessor.Should().NotBeNull();
    }

    [Test]
    public void When_AppIsRunning_With_TestApp_Result_RequestValidatorExists()
    {
        var validator1 = _context.GetService<IValidator<ChangeNameRequest>>();
        var validator2 = _context.GetService<IValidator<SomeBodyRequest>>();

        validator1.Should().NotBeNull();
        validator2.Should().NotBeNull();
    }

    [Test]
    public void When_AppIsRunning_With_TestApp_Result_ScopedCommandsExists()
    {
        var command = GetServiceIfExists<ICommand<AddEntityCommandContext, AddEntityCommandResult>>();
        using var scope = _context.CreateScope();
        var scopedCommand = scope.ServiceProvider
            .GetRequiredService<ICommand<AddEntityCommandContext, AddEntityCommandResult>>();

        command.Should().BeNull();
        scopedCommand.Should().NotBeNull();
    }

    [Test]
    public void When_AppIsRunning_With_TestApp_Result_ServiceClientExists()
    {
        var client = _context.GetService<ITestApplicationClient>();

        client.Should().NotBeNull();
    }

    [Test]
    public void When_AppIsRunning_With_TestApp_Result_AuthTokenResolverExists()
    {
        var manager = _context.GetService<IClientTokenManager<IAuthAppClient>>();

        manager.Should().NotBeNull();
    }

    [Test]
    public void When_AppIsRunning_With_TestApp_Result_DataModelContextFactoryExists()
    {
        var dataModelContext = _context.GetService<IDbContextFactory<DataModelContext>>();

        dataModelContext.Should().NotBeNull();
    }

    [Test]
    public void When_AppIsRunning_With_TestApp_Result_DataModelContextExists()
    {
        var dataModelContext = GetServiceIfExists<DataModelContext>();

        using var scope = _context.CreateScope();
        var context = scope.ServiceProvider.GetService<DataModelContext>();

        dataModelContext.Should().BeNull();
        context.Should().NotBeNull();
    }

    [Test]
    public void When_AppIsRunning_With_TestApp_Result_CommandExecutorExists()
    {
        var singletonExecutorExists = _context.GetService<ICommandExecutor>();
        var scopedExecutorDoesntExists = GetServiceIfExists<IScopedCommandExecutor>();

        using var scope = _context.CreateScope();
        var scopedExecutorExists = scope.ServiceProvider.GetService<IScopedCommandExecutor>();

        singletonExecutorExists.Should().NotBeNull();
        scopedExecutorExists.Should().NotBeNull();
        scopedExecutorDoesntExists.Should().BeNull();
    }

    private T? GetServiceIfExists<T>()
    where T : class
    {
        try
        {
            return _context.GetService<T>();
        }
        catch (InvalidOperationException)
        {
            return null;
        }
    }
}