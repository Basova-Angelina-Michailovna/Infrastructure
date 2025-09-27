using Flurl.Http;
using JetBrains.Annotations;
using Polly;

namespace StretchRoom.Infrastructure.HttpClient;

/// <summary>
///     The <see cref="DefaultClientPollyHelper" /> class.
/// </summary>
[PublicAPI]
public static class DefaultClientPollyHelper
{
    /// <summary>
    ///     Creates the default http executing policy.
    /// </summary>
    /// <param name="timeout">The timeout.</param>
    /// <param name="onRetryAction">The action that could be called on retry.</param>
    /// <typeparam name="TResult">The policy execution result.</typeparam>
    /// <returns>The new instance of configured policy.</returns>
    public static IAsyncPolicy<TResult> CreateDefaultHttpPolly<TResult>(TimeSpan timeout,
        Action<Exception, TimeSpan, int, Context>? onRetryAction = null)
    {
        var policy = Policy.Handle<FlurlHttpException>()
            .WaitAndRetryAsync([timeout], (ex, time, count, ctx) => { onRetryAction?.Invoke(ex, time, count, ctx); })
            .AsAsyncPolicy<TResult>();
        return policy;
    }
}