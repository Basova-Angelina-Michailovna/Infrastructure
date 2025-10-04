using Microsoft.Extensions.Logging;
using StretchRoom.Infrastructure.Extensions;

namespace StretchRoom.Infrastructure.Helpers;

/// <summary>
///     The <see cref="ScopedCommandExecutor" /> class.
/// </summary>
public class ScopedCommandExecutor(ILogger<ScopedCommandExecutor> logger, IServiceProvider serviceProvider)
    : IScopedCommandExecutor
{
    /// <inheritdoc />
    public async Task<TResult> ExecuteAsync<TContext, TResult>(TContext context, CancellationToken token = default)
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
}