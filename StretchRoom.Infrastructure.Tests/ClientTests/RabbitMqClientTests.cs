using AwesomeAssertions;
using StretchRoom.Infrastructure.AuthorizationTestApplication.BoundedContext;
using StretchRoom.Infrastructure.AuthorizationTestApplication.Client;
using StretchRoom.Tests.Infrastructure.Helpers;

namespace StretchRoom.Infrastructure.Tests.ClientTests;

public class RabbitMqClientTests
{
    private static readonly SrRandomizer Randomizer = SrRandomizer.Create();
    private IAppRabbitClient _client;

    [SetUp]
    public void Setup()
    {
        _client = AppTestContext.AppContext.Factory.Services.GetRequiredService<IAppRabbitClient>();
    }

    [Test]
    public async Task When_GetOk_With_Result_Ok()
    {
        var response = await _client.GetOkAsync();
        response.Should().NotBeEmpty();
    }

    [Test]
    public async Task When_GetJson_With_Result_Ok()
    {
        var request = new GenerateTokenRequest(Randomizer.String(16));
        var response = await _client.GenerateTokenAsync(request);
        response.Should().NotBeNull();
        response.Token.Should().NotBeNullOrEmpty();
    }

    [TestCase(10000)]
    public async Task LittleBenchmark(int iterations)
    {
        var times = new List<TimeSpan>(iterations);
        for (var i = 0; i < iterations; i++)
        {
            using var sw = StopWatchElapser.Create(time => times.Add(time));
            await When_GetJson_With_Result_Ok();
        }

        Console.WriteLine($"Sum: {TimeSpan.FromMilliseconds(times.Sum(t => t.TotalMilliseconds))}");
        Console.WriteLine($"Average: {TimeSpan.FromMilliseconds(times.Average(t => t.TotalMilliseconds))}");
    }
}