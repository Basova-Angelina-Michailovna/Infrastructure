using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace StretchRoom.Infrastructure.Models;

/// <summary>
///     The service api info.
/// </summary>
/// <param name="ServiceName">The service name.</param>
/// <param name="Description">The description.</param>
[PublicAPI]
public sealed record ServiceApiInfo(
    string ServiceName,
    PathString BaseAddress,
    string Description);