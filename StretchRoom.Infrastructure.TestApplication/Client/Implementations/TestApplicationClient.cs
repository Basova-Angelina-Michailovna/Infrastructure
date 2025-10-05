using Flurl.Http.Configuration;
using StretchRoom.Infrastructure.HttpClient;
using StretchRoom.Infrastructure.TestApplication.Client.Interfaces;

namespace StretchRoom.Infrastructure.TestApplication.Client.Implementations;

public class TestApplicationClient(
    IFlurlClientCache clientCache,
    ILoggerFactory loggerFactory,
    Func<string> baseUrlResolver,
    Func<Task<string>>? tokenResolver = null)
    : ClientBase(clientCache, loggerFactory, baseUrlResolver, tokenResolver), ITestApplicationClient
{
    public ITestClient TestClient { get; } = new TestClient(clientCache, loggerFactory, baseUrlResolver, tokenResolver);
}