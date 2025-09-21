using JetBrains.Annotations;

namespace StretchRoom.Infrastructure.Options;

/// <summary>
///     The <see cref="JwtOptions" /> class.
/// </summary>
[PublicAPI]
public class JwtOptions
{
    /// <summary>
    ///     The issuer.
    /// </summary>
    public required string Issuer { get; init; }

    /// <summary>
    ///     The audience.
    /// </summary>
    public required string Audience { get; init; }

    /// <summary>
    ///     The token time to live.
    /// </summary>
    public TimeSpan? TokenTimeToLive { get; init; }

    /// <summary>
    ///     The base 64 key.
    /// </summary>
    public required string Base64Key { get; init; }
}