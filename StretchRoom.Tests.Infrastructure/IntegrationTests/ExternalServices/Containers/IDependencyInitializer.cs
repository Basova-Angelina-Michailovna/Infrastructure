using JetBrains.Annotations;

namespace StretchRoom.Tests.Infrastructure.IntegrationTests.ExternalServices.Containers;

/// <summary>
///     The dependency initializer.
/// </summary>
[PublicAPI]
public interface IDependencyInitializer<TDependency>
{
    /// <summary>
    ///     Initialize the dependency.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    Task<TDependency> InitializeAsync(CancellationToken token = default);

    /// <summary>
    ///     Teardowns the dependency.
    /// </summary>
    /// <param name="token">The cancellation token.</param>
    /// <returns></returns>
    Task TeardownAsync(CancellationToken token = default);
}