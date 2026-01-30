using JetBrains.Annotations;
using Microsoft.AspNetCore.Http;

namespace StretchRoom.Infrastructure.HttpClient.TokenManager;

/// <summary>
///     The <see cref="IClientTokenManager{TInterface}" /> interface.
/// </summary>
/// <typeparam name="TInterface">The interface of client.</typeparam>
[PublicAPI]
public interface IClientTokenManager<TInterface>
{
    /// <summary>
    ///     Gets the auth token.
    /// </summary>
    /// <returns></returns>
    Task<string?> GetTokenAsync();
}

/// <summary>
///     The <see cref="FromContextClientTokenManager{TInterface}" /> class.
/// </summary>
/// <param name="contextAccessor">The http context accessor.</param>
/// <typeparam name="TInterface">The interface of client.</typeparam>
[PublicAPI]
public class FromContextClientTokenManager<TInterface>(IHttpContextAccessor contextAccessor)
    : IClientTokenManager<TInterface>
{
    /// <inheritdoc />
    public Task<string?> GetTokenAsync()
    {
        if (contextAccessor.HttpContext?.Request.Headers.TryGetValue("Authorization", out var token) ??
            false)
        {
            return Task.FromResult((string?)token);
        }

        return Task.FromResult<string?>(null);
    }
}