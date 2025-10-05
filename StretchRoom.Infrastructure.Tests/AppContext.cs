using StretchRoom.Infrastructure.TestApplication.Client.Implementations;

namespace StretchRoom.Infrastructure.Tests;

internal record AppContext(IServiceProvider ServiceProvider, ITestApplicationClient Client);