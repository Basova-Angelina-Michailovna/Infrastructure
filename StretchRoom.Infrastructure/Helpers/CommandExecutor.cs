using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using StretchRoom.Infrastructure.Extensions;

namespace StretchRoom.Infrastructure.Helpers;

/// <summary>
///     The <see cref="CommandExecutor" /> class.
/// </summary>
/// <param name="logger">The logger.</param>
/// <param name="serviceProvider">The service provider.</param>
[PublicAPI]
public class CommandExecutor(ILogger<CommandExecutor> logger, IServiceProvider serviceProvider) : ICommandExecutor
{
    /// <summary>
    ///     Executes the command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TContext">The context.</typeparam>
    /// <typeparam name="TResult">The result.</typeparam>
    /// <returns>The new instance of <typeparamref name="TResult" />.</returns>
    public async Task<TResult> ExecuteCommandAsync<TContext, TResult>(TContext context,
        CancellationToken token = default)
    {
        var commandName = typeof(TContext).Name;

        logger.LogDebug("{command} execute with context: {context}", commandName, context?.ToDiagnosticJson());

        var command = serviceProvider.GetRequiredCommand<TContext, TResult>();

        try
        {
            var result = await command.ExecuteAsync(context, token);

            logger.LogDebug("{command} execution result: {result}", commandName, result?.ToDiagnosticJson());
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{command} executed with exception", commandName);
            throw;
        }
    }

    /// <summary>
    ///     Executes the scoped command.
    /// </summary>
    /// <param name="context">The command context.</param>
    /// <param name="token">The cancellation token.</param>
    /// <typeparam name="TContext">The context.</typeparam>
    /// <typeparam name="TResult">The result.</typeparam>
    /// <returns>The new instance of <typeparamref name="TResult" />.</returns>
    public async Task<TResult> ExecuteScopedCommandAsync<TContext, TResult>(TContext context,
        CancellationToken token = default)
    {
        var commandName = typeof(TContext).Name;

        logger.LogDebug("{command} execute with context: {context}", commandName, context?.ToDiagnosticJson());

        await using var scope = serviceProvider.CreateAsyncScope();

        var command = scope.ServiceProvider.GetRequiredCommand<TContext, TResult>();

        try
        {
            var result = await command.ExecuteAsync(context, token);

            logger.LogDebug("{command} execution result: {result}", commandName, result?.ToDiagnosticJson());
            return result;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "{command} executed with exception", commandName);
            throw;
        }
    }
}