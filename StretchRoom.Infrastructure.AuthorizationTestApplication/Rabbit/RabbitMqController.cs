using System.Security.Claims;
using EBCEYS.RabbitMQ.Server.MappedService.Attributes;
using EBCEYS.RabbitMQ.Server.MappedService.SmartController;
using JetBrains.Annotations;
using StretchRoom.Infrastructure.AuthorizationTestApplication.BoundedContext;
using StretchRoom.Infrastructure.Helpers.Jwt;

namespace StretchRoom.Infrastructure.AuthorizationTestApplication.Rabbit;

using Routes = RoutesDictionary.RabbitMqControllerV1.Methods;

[UsedImplicitly]
[PublicAPI]
public class RabbitMqController(IJwtGenerator jwtGenerator) : RabbitMQSmartControllerBase
{
    [RabbitMQMethod(Routes.GetOk)]
    public Task<string> GetOk()
    {
        return Task.FromResult("Ok");
    }

    [RabbitMQMethod(Routes.GetJson)]
    public Task<GenerateTokenResponse> GenerateToken(GenerateTokenRequest request)
    {
        var token = jwtGenerator.GenerateKey(request.UserName.ToClaim(ClaimTypes.NameIdentifier));
        return Task.FromResult(new GenerateTokenResponse(token));
    }
}