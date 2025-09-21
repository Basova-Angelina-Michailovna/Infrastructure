using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace StretchRoom.Infrastructure.Models;

/// <summary>
///     The service api info.
/// </summary>
/// <param name="ServiceName">The service name.</param>
/// <param name="ApiVersions">
///     The api versions.
///     <br />
///     <example>[ "v1", "v2" etc.]</example>
/// </param>
/// <param name="Description">The description.</param>
[PublicAPI]
public sealed record ServiceApiInfo(
    string ServiceName,
    PathString BaseAddress,
    IReadOnlyCollection<string> ApiVersions,
    string Description);