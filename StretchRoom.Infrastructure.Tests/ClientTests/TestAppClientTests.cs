using AwesomeAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StretchRoom.Infrastructure.Exceptions;
using StretchRoom.Infrastructure.TestApplication.BoundedContext.Requests;
using StretchRoom.Infrastructure.TestApplication.Client.Implementations;
using StretchRoom.Infrastructure.TestApplication.DaL;
using StretchRoom.Tests.Infrastructure;

namespace StretchRoom.Infrastructure.Tests.ClientTests;

internal class TestAppClientTests
{
    private static readonly SrRandomizer Randomizer = new();
    private ITestApplicationClient _client;
    private AppContext _context;

    [SetUp]
    public void Setup()
    {
        _context = AppTestContext.AppContext;
        _client = _context.Client;
    }

    [TearDown]
    public async Task TearDown()
    {
        await using var dbContext = await _context.ServiceProvider
            .GetRequiredService<IDbContextFactory<DataModelContext>>()
            .CreateDbContextAsync();

        await dbContext.TestTable.Where(t => true).ExecuteDeleteAsync();
        await dbContext.SaveChangesAsync();
    }

    [Test]
    public async Task When_GetOk_With_Result_Ok()
    {
        var act = () => _client.TestClient.GetOkAsync(CancellationToken.None);
        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task When_GetException_With_Result_ApiExceptionAndCode500()
    {
        var act = () => _client.TestClient.GetExceptionAsync(CancellationToken.None);

        (await act.Should().ThrowAsync<ApiException>())
            .And.ProblemDetails.Status.Should().Be(500);
    }

    [Test]
    public async Task When_PostBody_With_ValidRequest_Result_Ok()
    {
        const string name = "Test";
        var request = new SomeBodyRequest(name);
        var response = await _client.TestClient.PostBodyAsync(request, CancellationToken.None);

        response.Should().NotBeNull();
        response.Message.Should().Be(name);
    }

    [Test]
    public async Task When_PostBody_With_InvalidRequest_Result_ApiExceptionAndCode400()
    {
        const string name = "";
        var request = new SomeBodyRequest(name);
        var act = () => _client.TestClient.PostBodyAsync(request, CancellationToken.None);

        (await act.Should().ThrowAsync<ApiException>())
            .And.ProblemDetails.Status.Should().Be(400);
    }

    [Test]
    public async Task When_GetQuery_With_RandomValue_Result_GetExactValue()
    {
        var value = Randomizer.Int(0);

        var result = await _client.TestClient.GetQueryAsync(value, CancellationToken.None);

        result.Should().NotBeNull();
        result.Message.Should().Be(value.ToString());
    }

    [Test]
    public async Task When_PostCommand_With_RandomName_Result_CommandExecutedSuccessfully()
    {
        var name = Randomizer.String(10);
        var response = await _client.TestClient.PostCommandAsync(name, CancellationToken.None);

        response.Should().NotBeNull();
        response.Entities.Should().HaveCount(1)
            .And.Contain(e => e.Name == name);
    }

    [Test]
    public async Task When_GetCommand_With_Result_Ok()
    {
        var emptyResponse = await _client.TestClient.GetCommandAsync(CancellationToken.None);

        await _client.TestClient.PostCommandAsync(Randomizer.String(10), CancellationToken.None);

        var oneResponse = await _client.TestClient.GetCommandAsync(CancellationToken.None);

        emptyResponse.Should().NotBeNull();
        emptyResponse.Entities.Should().HaveCount(0);

        oneResponse.Should().NotBeNull();
        oneResponse.Entities.Should().HaveCount(1);
    }

    [Test]
    public async Task When_DeleteCommand_With_NonExistingEntity_Result_ApiExceptionAndCode404()
    {
        var act = () => _client.TestClient.DeleteCommandAsync(Randomizer.String(10), CancellationToken.None);

        (await act.Should().ThrowAsync<ApiException>())
            .And.ProblemDetails.Status.Should().Be(404);
    }

    [Test]
    public async Task When_DeleteCommand_With_ExistingEntity_Result_StatusCode200()
    {
        var name = Randomizer.String(10);
        await _client.TestClient.PostCommandAsync(name, CancellationToken.None);

        var act = () => _client.TestClient.DeleteCommandAsync(name, CancellationToken.None);

        await act.Should().NotThrowAsync<ApiException>();
    }

    [Test]
    public async Task When_PutCommand_With_ExistingEntity_Result_StatusCode200()
    {
        var name = Randomizer.String(10);
        await _client.TestClient.PostCommandAsync(name, CancellationToken.None);

        var newName = Randomizer.String(10);
        var act = () =>
            _client.TestClient.PutCommandAsync(name, new ChangeNameRequest(newName), CancellationToken.None);

        await act.Should().NotThrowAsync<ApiException>();

        var response = await _client.TestClient.GetCommandAsync(CancellationToken.None);

        response.Should().NotBeNull();
        response.Entities.Should().HaveCount(1);
        response.Entities.Should().Contain(e => e.Name == newName);
    }
}