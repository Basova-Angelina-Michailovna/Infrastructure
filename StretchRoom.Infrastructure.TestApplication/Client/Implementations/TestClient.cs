using Flurl.Http.Configuration;
using Microsoft.AspNetCore.Mvc;
using StretchRoom.Infrastructure.AuthorizationTestApplication.BoundedContext;
using StretchRoom.Infrastructure.Exceptions;
using StretchRoom.Infrastructure.HttpClient;
using StretchRoom.Infrastructure.TestApplication.BoundedContext.Requests;
using StretchRoom.Infrastructure.TestApplication.BoundedContext.Responses;
using StretchRoom.Infrastructure.TestApplication.Client.Interfaces;
using RoutesDictionary = StretchRoom.Infrastructure.TestApplication.BoundedContext.RoutesDictionary;

namespace StretchRoom.Infrastructure.TestApplication.Client.Implementations;

public class TestClient(
    IFlurlClientCache clientCache,
    ILoggerFactory loggerFactory,
    Func<string> baseUrlResolver,
    Func<Task<string>>? tokenResolver = null)
    : ClientBase(clientCache, loggerFactory, baseUrlResolver, tokenResolver), ITestClient
{
    private const string BasePath = RoutesDictionary.TestControllerV1.BaseRoute;

    public async Task GetOkAsync(CancellationToken token)
    {
        var result = await GetAsync<ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.GetOk), token: token);

        if (!result.IsSuccess) ApiExceptionHelper.ThrowApiException(result.Error, result.StatusCode);
    }

    public async Task GetExceptionAsync(CancellationToken token)
    {
        var result = await GetAsync<ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.GetException),
            token: token);

        if (!result.IsSuccess) ApiExceptionHelper.ThrowApiException(result.Error, result.StatusCode);
    }

    public async Task<SomeBodyResponse> PostBodyAsync(SomeBodyRequest body, CancellationToken token)
    {
        var result = await PostJsonAsync<SomeBodyRequest, SomeBodyResponse, ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.PostBody),
            body,
            token: token);
        if (result is { IsSuccess: true, Result: not null }) return result.Result;

        return ApiExceptionHelper.ThrowApiException<SomeBodyResponse>(result.Error, result.StatusCode);
    }

    public async Task<SomeBodyResponse> GetQueryAsync(int value, CancellationToken token)
    {
        var result = await GetJsonAsync<SomeBodyResponse, ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.GetQuery)
                .AppendQueryParam("value", value),
            token: token);

        if (result is { IsSuccess: true, Result: not null }) return result.Result;

        return ApiExceptionHelper.ThrowApiException<SomeBodyResponse>(result.Error, result.StatusCode);
    }

    public async Task<CommandResultResponse> PostCommandAsync(string name, CancellationToken token)
    {
        var result = await PostJsonAsync<CommandResultResponse, ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.PostCommand)
                .AppendQueryParam("name", name),
            token: token);

        if (result is { IsSuccess: true, Result: not null }) return result.Result;

        return ApiExceptionHelper.ThrowApiException<CommandResultResponse>(result.Error, result.StatusCode);
    }

    public async Task<CommandResultResponse> GetCommandAsync(CancellationToken token)
    {
        var result = await GetJsonAsync<CommandResultResponse, ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.GetCommand),
            token: token);

        if (result is { IsSuccess: true, Result: not null }) return result.Result;

        return ApiExceptionHelper.ThrowApiException<CommandResultResponse>(result.Error, result.StatusCode);
    }

    public async Task DeleteCommandAsync(string name, CancellationToken token)
    {
        var result = await DeleteAsync<ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.DeleteCommand)
                .AppendQueryParam("name", name),
            token: token);

        if (!result.IsSuccess) ApiExceptionHelper.ThrowApiException(result.Error, result.StatusCode);
    }

    public async Task PutCommandAsync(string name, ChangeNameRequest request, CancellationToken token)
    {
        var result = await PutJsonAsync<ChangeNameRequest, ProblemDetails>(url => url
                .AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.PutCommand)
                .AppendQueryParam("name", name),
            request,
            token: token);

        if (!result.IsSuccess) ApiExceptionHelper.ThrowApiException(result.Error, result.StatusCode);
    }

    public async Task<GenerateTokenResponse> GenerateTokenAsync(CancellationToken token)
    {
        var result = await GetJsonAsync<GenerateTokenResponse, ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.GetToken),
            token: token);
        
        if (!result.IsSuccess || result.Result is null)
        {
            return ApiExceptionHelper.ThrowApiException<GenerateTokenResponse>(result.Error, result.StatusCode);
        }

        return result.Result;
    }

    public async Task ValidateTokenAsync(string jwt, CancellationToken token)
    {
        var result = await GetAsync<ProblemDetails>(
            url => url.AppendPathSegments(BasePath, RoutesDictionary.TestControllerV1.Methods.ValidateToken),
            new Dictionary<string, object>
            {
                { "Authorization", jwt }
            }, token);

        if (!result.IsSuccess)
        {
            ApiExceptionHelper.ThrowApiException(result.Error, result.StatusCode);
        }
    }
}