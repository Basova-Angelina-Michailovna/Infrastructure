using AwesomeAssertions;
using StretchRoom.Infrastructure.Exceptions;
using StretchRoom.Infrastructure.TestApplication.Client.Implementations;
using StretchRoom.Tests.Infrastructure.Helpers;

namespace StretchRoom.Infrastructure.Tests.ClientTests;

public class AuthClientTests
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

    [Test]
    public async Task When_GetToken_With_AppClient_Result_Ok()
    {
        var result = await _client.TestClient.GenerateTokenAsync(CancellationToken.None);

        result.Should().NotBeNull();
        result.Token.Should().NotBeNullOrEmpty();
        Console.WriteLine(result.Token);
    }

    [Test]
    public async Task When_ValidateToken_With_GotToken_Result_Ok()
    {
        var token = await _client.TestClient.GenerateTokenAsync(CancellationToken.None);

        var act = () => _client.TestClient.ValidateTokenAsync(token.Token, CancellationToken.None);

        await act.Should().NotThrowAsync();
    }

    [Test]
    public async Task When_ValidateToken_With_RandomToken_Result_Unauthorized()
    {
        var act = () => _client.TestClient.ValidateTokenAsync(Randomizer.String(64), CancellationToken.None);

        (await act.Should().ThrowAsync<ApiException>())
            .And.ProblemDetails.Status.Should().Be(StatusCodes.Status403Forbidden);
    }
}