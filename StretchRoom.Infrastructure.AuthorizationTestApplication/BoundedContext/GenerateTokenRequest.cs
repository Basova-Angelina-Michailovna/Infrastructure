using System.Security.Claims;

namespace StretchRoom.Infrastructure.AuthorizationTestApplication.BoundedContext;

public record GenerateTokenRequest(string UserName)
{
    public const string UserNameClaimType = ClaimTypes.NameIdentifier;
};

public record GenerateTokenResponse(string Token);