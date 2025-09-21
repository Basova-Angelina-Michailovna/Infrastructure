using JetBrains.Annotations;

namespace StretchRoom.Infrastructure.Interfaces;

/// <summary>
///     The <see cref="ICommand{TContext,TResult}" /> interface.
/// </summary>
/// <typeparam name="TContext">The command context.</typeparam>
/// <typeparam name="TResult">The command execution result.</typeparam>
[PublicAPI]
public interface ICommand<in TContext, TResult>
{
    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="token">The cancellation token.</param>
    /// <returns>The <see cref="Task{TResult}" /> with specified result instance of <see cref="TResult" />.</returns>
    Task<TResult> ExecuteAsync(TContext context, CancellationToken token = default);
}