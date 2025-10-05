using StretchRoom.Infrastructure.TestApplication.Client.Interfaces;

namespace StretchRoom.Infrastructure.TestApplication.Client.Implementations;

public interface ITestApplicationClient
{
    ITestClient TestClient { get; }
}