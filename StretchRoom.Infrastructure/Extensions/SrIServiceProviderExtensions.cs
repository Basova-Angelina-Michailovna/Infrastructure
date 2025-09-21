using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using StretchRoom.Infrastructure.Interfaces;

namespace StretchRoom.Infrastructure.Extensions;

/// <summary>
/// The <see cref="IServiceProvider"/> extensions.
/// </summary>
[PublicAPI]
public static class SrIServiceProviderExtensions
{
    /// <summary>
    /// Gets the command from service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <typeparam name="TContext">The command context.</typeparam>
    /// <typeparam name="TResult">The command result.</typeparam>
    /// <returns>The instance of <see cref="ICommand{TContext,TResult}"/> if exists; otherwise <c>null</c>.</returns>
    public static ICommand<TContext, TResult>? GetCommand<TContext, TResult>(this IServiceProvider serviceProvider)
    {
        return serviceProvider.GetService<ICommand<TContext, TResult>>();
    }

    /// <summary>
    /// Gets the command from service provider.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <typeparam name="TContext">The command context.</typeparam>
    /// <typeparam name="TResult">The command result.</typeparam>
    /// <returns>The instance of <see cref="ICommand{TContext,TResult}"/>.</returns>
    public static ICommand<TContext, TResult> GetRequiredCommand<TContext, TResult>(
        this IServiceProvider serviceProvider)
    {
        return serviceProvider.GetRequiredService<ICommand<TContext, TResult>>();
    }
}