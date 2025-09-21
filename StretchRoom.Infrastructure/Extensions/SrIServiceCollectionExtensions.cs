using FluentValidation;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using StretchRoom.Infrastructure.Interfaces;

namespace StretchRoom.Infrastructure.Extensions;

/// <summary>
/// The <see cref="IServiceCollection"/> extensions.
/// </summary>
[PublicAPI]
public static class SrIServiceCollectionExtensions
{
    /// <summary>
    /// Adds the <typeparamref name="TValidator"/> as <see cref="IValidator{TBody}"/> to transient services.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <typeparam name="TValidator">The abstract validator.</typeparam>
    /// <typeparam name="TBody">The body.</typeparam>
    /// <returns>The <paramref name="services"/> instance.</returns>
    public static IServiceCollection AddValidator<TValidator, TBody>(this IServiceCollection services)
        where TValidator : AbstractValidator<TBody>
    {
        services.AddTransient<IValidator<TBody>, TValidator>();
        return services;
    }

    /// <summary>
    /// Adds command to <paramref name="services"/> as scoped service.
    /// </summary>
    /// <param name="services">The service.</param>
    /// <typeparam name="TCommand">The command implementation.</typeparam>
    /// <typeparam name="TContext">The command context.</typeparam>
    /// <typeparam name="TResult">The command result.</typeparam>
    /// <returns>The <paramref name="services"/> instance.</returns>
    public static IServiceCollection AddScopedCommand<TCommand, TContext, TResult>(this IServiceCollection services)
        where TCommand : class, ICommand<TContext, TResult>
    {
        services.AddScoped<ICommand<TContext, TResult>, TCommand>();
        return services;
    }

    /// <summary>
    /// Adds command to <paramref name="services"/> as singleton service.
    /// </summary>
    /// <param name="services">The service.</param>
    /// <typeparam name="TCommand">The command implementation.</typeparam>
    /// <typeparam name="TContext">The command context.</typeparam>
    /// <typeparam name="TResult">The command result.</typeparam>
    /// <returns>The <paramref name="services"/> instance.</returns>
    public static IServiceCollection AddSingletonCommand<TCommand, TContext, TResult>(this IServiceCollection services)
        where TCommand : class, ICommand<TContext, TResult>
    {
        services.AddSingleton<ICommand<TContext, TResult>, TCommand>();
        return services;
    }
    
    /// <summary>
    /// Adds command to <paramref name="services"/> as transient service.
    /// </summary>
    /// <param name="services">The service.</param>
    /// <typeparam name="TCommand">The command implementation.</typeparam>
    /// <typeparam name="TContext">The command context.</typeparam>
    /// <typeparam name="TResult">The command result.</typeparam>
    /// <returns>The <paramref name="services"/> instance.</returns>
    public static IServiceCollection AddTransientCommand<TCommand, TContext, TResult>(this IServiceCollection services)
        where TCommand : class, ICommand<TContext, TResult>
    {
        services.AddTransient<ICommand<TContext, TResult>, TCommand>();
        return services;
    }
}