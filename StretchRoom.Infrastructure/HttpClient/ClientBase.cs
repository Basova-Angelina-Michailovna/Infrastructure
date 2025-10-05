using System.Diagnostics;
using Flurl;
using Flurl.Http;
using Flurl.Http.Configuration;
using Flurl.Http.Content;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using Polly;

namespace StretchRoom.Infrastructure.HttpClient;

/// <summary>
///     The <see cref="ClientBase" /> class.
/// </summary>
[PublicAPI]
public abstract class ClientBase
{
    /// <summary>
    ///     The authorization header key.
    /// </summary>
    public const string AuthorizationHeader = "Authorization";

    private readonly IFlurlClient _client;
    private readonly ILogger _logger;

    private readonly Func<Task<string>>? _tokenResolver;

    /// <summary>
    ///     Initiates the new instance of <see cref="ClientBase" />.
    /// </summary>
    /// <param name="clientCache">The client cache.</param>
    /// <param name="baseUrlResolver">The base url resolver.</param>
    /// <param name="tokenResolver">The token resolver.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    protected ClientBase(
        IFlurlClientCache clientCache,
        ILoggerFactory loggerFactory,
        Func<string> baseUrlResolver,
        Func<Task<string>>? tokenResolver = null)
    {
        _tokenResolver = tokenResolver;
        Policy = DefaultClientPollyHelper.CreateDefaultHttpPolly<IFlurlResponse>();
        _client = clientCache.GetOrAdd(ClientName, baseUrlResolver(), ConfigureClient);
        _logger = loggerFactory.CreateLogger(GetType());
    }

    /// <summary>
    ///     The client name to get it from <see cref="IFlurlClientCache" />.
    /// </summary>
    protected string ClientName => GetClientName(GetType());

    /// <summary>
    ///     The default response awaiting timeout.
    /// </summary>
    protected TimeSpan Timeout { get; init; } = TimeSpan.FromSeconds(10);

    /// <summary>
    ///     The default http request executing policy.
    /// </summary>
    protected IAsyncPolicy<IFlurlResponse> Policy { get; init; }

    /// <summary>
    ///     The default request and response serializer.
    /// </summary>
    public ISerializer DefaultSerializer { get; init; } = new DefaultJsonSerializer();

    private void ConfigureClient(IFlurlClientBuilder obj)
    {
        obj.AllowAnyHttpStatus();
        obj.WithTimeout(Timeout);
        obj.WithSettings(act => { act.JsonSerializer = DefaultSerializer; });
    }

    /// <summary>
    /// Gets the client name. With this name the client will get from <see cref="IFlurlClientCache"/>.
    /// </summary>
    /// <param name="implementationType">The client implementation type.</param>
    /// <returns></returns>
    public static string GetClientName(Type implementationType)
    {
        return $"{implementationType.Name}{nameof(ClientBase)}";
    }

    /// <summary>
    ///     Sends GET request with awaiting serialized body response.
    /// </summary>
    /// <param name="url">The url transform.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TResponse">The success response type.</typeparam>
    /// <typeparam name="TError">The error response type.</typeparam>
    /// <returns>The new instance of <see cref="OperationResult{TResponse,TError}" />.</returns>
    protected async Task<OperationResult<TResponse, TError>> GetJsonAsync<TResponse, TError>(
        Action<Url> url,
        IDictionary<string, object>? headers = null,
        CancellationToken token = default) where TResponse : class where TError : class
    {
        var request = await PrepareRequest(url, headers);

        var response = await ExecuteWithPolicyAsync(request, HttpMethod.Get, token: token);

        return await OperationResult<TResponse, TError>.CreateFromResponseAsync(response);
    }

    /// <summary>
    ///     Sends GET request.
    /// </summary>
    /// <param name="url">The url transform.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TError">The error response type.</typeparam>
    /// <returns>The new instance of <see cref="OperationResult{TResponse,TError}" />.</returns>
    protected async Task<OperationResult<TError>> GetAsync<TError>(
        Action<Url> url,
        IDictionary<string, object>? headers = null,
        CancellationToken token = default) where TError : class
    {
        var request = await PrepareRequest(url, headers);

        var response = await ExecuteWithPolicyAsync(request, HttpMethod.Get, token: token);

        return await OperationResult<TError>.CreateFromResponseAsync(response);
    }

    /// <summary>
    ///     Sends GET request with awaiting <see cref="Stream" /> response.
    /// </summary>
    /// <param name="url">The url transform.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TError">The error response type.</typeparam>
    /// <returns>The new instance of <see cref="OperationResult{TResponse,TError}" />.</returns>
    protected async Task<OperationResult<Stream, TError>> GetStreamAsync<TError>(
        Action<Url> url,
        IDictionary<string, object>? headers = null,
        CancellationToken token = default) where TError : class
    {
        var request = await PrepareRequest(url, headers);

        var response =
            await ExecuteWithPolicyAsync(request, HttpMethod.Get, HttpCompletionOption.ResponseHeadersRead, token);

        return await OperationResult<Stream, TError>.CreateFromStreamResponseAsync(response);
    }

    /// <summary>
    ///     Sends GET request with awaiting <see cref="byte" /> array response.
    /// </summary>
    /// <param name="url">The url transform.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TError">The error response type.</typeparam>
    /// <returns>The new instance of <see cref="OperationResult{TResponse,TError}" />.</returns>
    protected async Task<OperationResult<byte[], TError>> GetRawAsync<TError>(
        Action<Url> url,
        IDictionary<string, object>? headers = null,
        CancellationToken token = default) where TError : class
    {
        var request = await PrepareRequest(url, headers);

        var response = await ExecuteWithPolicyAsync(request, HttpMethod.Get, token: token);

        return await OperationResult<byte[], TError>.CreateFromRawResponseAsync(response);
    }

    /// <summary>
    ///     Sends DELETE request with awaiting serialized body response.
    /// </summary>
    /// <param name="url">The url transform.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TResponse">The success response type.</typeparam>
    /// <typeparam name="TError">The error response type.</typeparam>
    /// <returns>The new instance of <see cref="OperationResult{TResponse,TError}" />.</returns>
    protected async Task<OperationResult<TResponse, TError>> DeleteJsonAsync<TResponse, TError>(
        Action<Url> url,
        IDictionary<string, object>? headers = null,
        CancellationToken token = default) where TResponse : class where TError : class
    {
        var request = await PrepareRequest(url, headers);

        var response = await ExecuteWithPolicyAsync(request, HttpMethod.Delete, token: token);

        return await OperationResult<TResponse, TError>.CreateFromResponseAsync(response);
    }

    /// <summary>
    ///     Sends DELETE request.
    /// </summary>
    /// <param name="url">The url transform.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TError">The error response type.</typeparam>
    /// <returns>The new instance of <see cref="OperationResult{TResponse,TError}" />.</returns>
    protected async Task<OperationResult<TError>> DeleteAsync<TError>(
        Action<Url> url,
        IDictionary<string, object>? headers = null,
        CancellationToken token = default) where TError : class
    {
        var request = await PrepareRequest(url, headers);

        var response = await ExecuteWithPolicyAsync(request, HttpMethod.Delete, token: token);

        return await OperationResult<TError>.CreateFromResponseAsync(response);
    }

    /// <summary>
    ///     Sends POST request with specified serialized body.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <param name="body">The body.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TRequest">The request object type.</typeparam>
    /// <typeparam name="TError">The error response type.</typeparam>
    /// <returns>The new instance of <see cref="OperationResult{TError}" />.</returns>
    protected async Task<OperationResult<TError>> PostJsonAsync<TRequest, TError>(
        Action<Url> url,
        TRequest body,
        IDictionary<string, object>? headers = null,
        CancellationToken token = default) where TError : class
    {
        var request = await PrepareRequest(url, headers);

        var serializedBody = DefaultSerializer.Serialize(body);
        request.Content = new CapturedJsonContent(serializedBody);

        var response = await ExecuteWithPolicyAsync(request, HttpMethod.Post, token: token);

        return await OperationResult<TError>.CreateFromResponseAsync(response);
    }

    /// <summary>
    ///     Sends POST request with specified stream body <br />
    ///     and with awaiting <typeparamref name="TResponse" /> deserialized
    ///     response.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <param name="body">The body.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TError">The error response type.</typeparam>
    /// <typeparam name="TResponse">The success response type.</typeparam>
    /// <returns>The new instance of <see cref="OperationResult{TError}" />.</returns>
    protected async Task<OperationResult<TResponse, TError>> PostStreamAsync<TResponse, TError>(
        Action<Url> url,
        Stream body,
        IDictionary<string, object>? headers = null,
        CancellationToken token = default) where TError : class where TResponse : class
    {
        var request = await PrepareRequest(url, headers);

        request.Content = new StreamContent(body);

        var response = await ExecuteWithPolicyAsync(request, HttpMethod.Post, token: token);

        return await OperationResult<TResponse, TError>.CreateFromResponseAsync(response);
    }

    /// <summary>
    ///     Sends POST request with specified stream body.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <param name="body">The body.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TError">The error response type.</typeparam>
    /// <returns>The new instance of <see cref="OperationResult{TError}" />.</returns>
    protected async Task<OperationResult<TError>> PostStreamAsync<TError>(
        Action<Url> url,
        Stream body,
        IDictionary<string, object>? headers = null,
        CancellationToken token = default) where TError : class
    {
        var request = await PrepareRequest(url, headers);

        request.Content = new StreamContent(body);

        var response = await ExecuteWithPolicyAsync(request, HttpMethod.Post, token: token);

        return await OperationResult<TError>.CreateFromResponseAsync(response);
    }

    /// <summary>
    ///     Sends POST request with specified serialized body <br />
    ///     and with awaiting <typeparamref name="TResponse" /> deserialized
    ///     response.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <param name="body">The body.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TRequest">The request object type.</typeparam>
    /// <typeparam name="TError">The error response type.</typeparam>
    /// <typeparam name="TResponse">The success response type.</typeparam>
    /// <returns>The new instance of <see cref="OperationResult{TError}" />.</returns>
    protected async Task<OperationResult<TResponse, TError>> PostJsonAsync<TRequest, TResponse, TError>(
        Action<Url> url,
        TRequest body,
        IDictionary<string, object>? headers = null,
        CancellationToken token = default) where TError : class where TResponse : class
    {
        var request = await PrepareRequest(url, headers);

        var serializedBody = DefaultSerializer.Serialize(body);
        request.Content = new CapturedJsonContent(serializedBody);

        var response = await ExecuteWithPolicyAsync(request, HttpMethod.Post, token: token);

        return await OperationResult<TResponse, TError>.CreateFromResponseAsync(response);
    }

    /// <summary>
    ///     Sends POST request<br />
    ///     and with awaiting <typeparamref name="TResponse" /> deserialized
    ///     response.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TError">The error response type.</typeparam>
    /// <typeparam name="TResponse">The success response type.</typeparam>
    /// <returns>The new instance of <see cref="OperationResult{TError}" />.</returns>
    protected async Task<OperationResult<TResponse, TError>> PostJsonAsync<TResponse, TError>(
        Action<Url> url,
        IDictionary<string, object>? headers = null,
        CancellationToken token = default) where TError : class where TResponse : class
    {
        var request = await PrepareRequest(url, headers);

        var response = await ExecuteWithPolicyAsync(request, HttpMethod.Post, token: token);

        return await OperationResult<TResponse, TError>.CreateFromResponseAsync(response);
    }

    /// <summary>
    ///     Sends POST request.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TError">The error response type.</typeparam>
    /// <returns>The new instance of <see cref="OperationResult{TError}" />.</returns>
    protected async Task<OperationResult<TError>> PostJsonAsync<TError>(
        Action<Url> url,
        IDictionary<string, object>? headers = null,
        CancellationToken token = default) where TError : class
    {
        var request = await PrepareRequest(url, headers);

        var response = await ExecuteWithPolicyAsync(request, HttpMethod.Post, token: token);

        return await OperationResult<TError>.CreateFromResponseAsync(response);
    }

    /// <summary>
    ///     Sends PUT request with specified serialized body.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <param name="body">The body.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TRequest">The request object type.</typeparam>
    /// <typeparam name="TError">The error response type.</typeparam>
    /// <returns>The new instance of <see cref="OperationResult{TError}" />.</returns>
    protected async Task<OperationResult<TError>> PutJsonAsync<TRequest, TError>(
        Action<Url> url,
        TRequest body,
        IDictionary<string, object>? headers = null,
        CancellationToken token = default) where TError : class
    {
        var request = await PrepareRequest(url, headers);

        var serializedBody = DefaultSerializer.Serialize(body);
        request.Content = new CapturedJsonContent(serializedBody);

        var response = await ExecuteWithPolicyAsync(request, HttpMethod.Put, token: token);

        return await OperationResult<TError>.CreateFromResponseAsync(response);
    }

    /// <summary>
    ///     Sends PUT request with specified serialized body and with awaiting <typeparamref name="TResponse" /> deserialized
    ///     response.
    /// </summary>
    /// <param name="url">The url.</param>
    /// <param name="body">The body.</param>
    /// <param name="headers">The headers.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TRequest">The request object type.</typeparam>
    /// <typeparam name="TError">The error response type.</typeparam>
    /// <typeparam name="TResponse">The success response type.</typeparam>
    /// <returns>The new instance of <see cref="OperationResult{TError}" />.</returns>
    protected async Task<OperationResult<TResponse, TError>> PutJsonAsync<TRequest, TResponse, TError>(
        Action<Url> url,
        TRequest body,
        IDictionary<string, object>? headers = null,
        CancellationToken token = default) where TError : class where TResponse : class
    {
        var request = await PrepareRequest(url, headers);

        var serializedBody = DefaultSerializer.Serialize(body);
        request.Content = new CapturedJsonContent(serializedBody);

        var response = await ExecuteWithPolicyAsync(request, HttpMethod.Put, token: token);

        return await OperationResult<TResponse, TError>.CreateFromResponseAsync(response);
    }

    /// <summary>
    ///     Executes the flurl request with <see cref="Policy" />.
    /// </summary>
    /// <param name="request">The request.</param>
    /// <param name="method">The http method.</param>
    /// <param name="completionOption">The completion options.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The <see cref="Task" /> which result contains the new instance of <see cref="IFlurlResponse" />.</returns>
    protected async Task<IFlurlResponse> ExecuteWithPolicyAsync(
        IFlurlRequest request,
        HttpMethod method,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken token = default)
    {
        var requestId = Guid.CreateVersion7().ToString("N").ToUpper();
        LogRequest(method, request, requestId);
        var stopwatch = Stopwatch.StartNew();
        var result = await Policy.ExecuteAsync(async () =>
        {
            return await ExecuteWithoutPolicyAsync(
                (r, t) => r.SendAsync(method, request.Content, completionOption, t),
                request,
                token);
        });
        stopwatch.Stop();
        LogResponse(method, result, request, stopwatch.Elapsed, requestId);
        return result;
    }

    /// <summary>
    ///     Executes the request delegate without policy.
    /// </summary>
    /// <param name="func">The delegate to execute.</param>
    /// <param name="request">The request to be executed.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The <see cref="Task" /> which result contains the new instance of <see cref="IFlurlResponse" />.</returns>
    protected virtual Task<IFlurlResponse> ExecuteWithoutPolicyAsync(
        Func<IFlurlRequest, CancellationToken, Task<IFlurlResponse>> func,
        IFlurlRequest request,
        CancellationToken token = default)
    {
        return func.Invoke(request, token);
    }

    private async Task<IFlurlRequest> PrepareRequest(Action<Url> url, IDictionary<string, object>? headers)
    {
        var requestHeaders = await GetDefaultHeaders(headers);
        var request = _client.Request().WithHeaders(requestHeaders);
        url.Invoke(request.Url);
        //var uri = new Url(_client.BaseUrl);
        //url.Invoke(uri);
        //var req = new FlurlRequest(uri).WithHeaders(requestHeaders);
        //req.Client = _client;
        //url.Invoke(request.Url);
        return request;
    }

    private async Task<Dictionary<string, object>> GetDefaultHeaders(
        IDictionary<string, object>? additionalHeaders = null)
    {
        var headers = new Dictionary<string, object>();
        if (_tokenResolver is not null)
        {
            var token = await _tokenResolver.Invoke();
            headers.Add(AuthorizationHeader, token);
        }

        if (additionalHeaders == null) return headers;
        foreach (var header in additionalHeaders) headers.Add(header.Key, header.Value);
        return headers;
    }

    private void LogRequest(
        HttpMethod method,
        IFlurlRequest request,
        string requestId)
    {
        if (_logger.IsEnabled(LogLevel.Debug)
            && request.Content is CapturedJsonContent content)
        {
            _logger.LogDebug("[Send {id}] {method} {url} with body:\n{body}", requestId, method, request.Url,
                content.Content);
            return;
        }

        _logger.LogInformation("[Send {id}] {method} {url}", requestId, method, request.Url);
    }

    private void LogResponse(
        HttpMethod method,
        IFlurlResponse response,
        IFlurlRequest request,
        TimeSpan duration,
        string requestId)
    {
        if (_logger.IsEnabled(LogLevel.Debug)
            && response.ResponseMessage.Content is CapturedJsonContent content)
        {
            _logger.LogDebug("[Receive {id}] {method} {url} {statusCode} for {duration}ms with body:\n{body}",
                requestId,
                method,
                request.Url,
                response.StatusCode,
                duration.TotalMilliseconds,
                content.Content);
            return;
        }

        _logger.LogInformation("[Receive {id}] {method} {url} {statusCode} for {duration}ms",
            requestId,
            method,
            request.Url,
            response.StatusCode,
            duration.TotalMilliseconds);
    }
}