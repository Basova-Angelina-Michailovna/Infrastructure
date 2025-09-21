namespace StretchRoom.Infrastructure.Helpers;

/// <summary>
/// The <see cref="ICommandExecutor"/> interface.
/// </summary>
public interface ICommandExecutor
{
    /// <summary>
    /// Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TContext">The context.</typeparam>
    /// <typeparam name="TResult">The result.</typeparam>
    /// <returns>The new instance of <typeparamref name="TResult"/>.</returns>
    Task<TResult> ExecuteCommandAsync<TContext, TResult>(TContext context, CancellationToken token = default);

    /// <summary>
    /// Executes the scoped command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TContext">The context.</typeparam>
    /// <typeparam name="TResult">The result.</typeparam>
    /// <returns>The new instance of <typeparamref name="TResult"/>.</returns>
    Task<TResult> ExecuteScopedCommandAsync<TContext, TResult>(TContext context, CancellationToken token = default);
}