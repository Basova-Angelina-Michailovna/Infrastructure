using System.Diagnostics.CodeAnalysis;
using System.IdentityModel.Tokens.Jwt;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;

namespace StretchRoom.Infrastructure.Helpers.Jwt;

/// <summary>
///     The <see cref="IJwtValidator" /> interface.
/// </summary>
[PublicAPI]
public interface IJwtValidator
{
    /// <summary>
    ///     Tries to validate <paramref name="token" />.
    /// </summary>
    /// <param name="token">The token.</param>
    /// <param name="jwtSecurityToken">The <see cref="JwtSecurityToken" /> if validated successfully.</param>
    /// <returns></returns>
    bool TryValidate(string token, [NotNullWhen(true)] out JwtSecurityToken? jwtSecurityToken);
}

/// <summary>
///     The <see cref="JwtValidator" /> class.
/// </summary>
/// <param name="logger">The logger.</param>
[PublicAPI]
public class JwtValidator(ILogger<JwtValidator> logger) : IJwtValidator
{
    /// <inheritdoc />
    public bool TryValidate(string token, [NotNullWhen(true)] out JwtSecurityToken? jwtSecurityToken)
    {
        try
        {
            var jwt = token.Replace($"{JwtGenerator.AuthSchema} ", "");
            jwtSecurityToken = new JwtSecurityTokenHandler().ReadJwtToken(jwt);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Error on validating jwt");
            jwtSecurityToken = null;
            return false;
        }
    }
}