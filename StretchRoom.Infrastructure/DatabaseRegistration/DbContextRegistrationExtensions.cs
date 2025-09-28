using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace StretchRoom.Infrastructure.DatabaseRegistration;

/// <summary>
///     The <see cref="DbContextRegistrationExtensions" /> extension class.
/// </summary>
[PublicAPI]
public static class DbContextRegistrationExtensions
{
    /// <summary>
    ///     Registers the <typeparamref name="TDbContext" /> in <see cref="services" />.
    /// </summary>
    /// <param name="services">The services.</param>
    /// <param name="registrationOptions">The db context registration options.</param>
    /// <typeparam name="TDbContext">The DbContext.</typeparam>
    public static void RegisterDbContext<TDbContext>(
        this IServiceCollection services,
        Action<DbContextRegistrationOptions> registrationOptions)
        where TDbContext : DbContext
    {
        var options = new DbContextRegistrationOptions();
        registrationOptions(options);
        services.AddDbContextFactory<TDbContext>(opts
            => opts.UseNpgsql(options.ConnectionString, builder =>
            {
                if (options.Retries is not null) builder.EnableRetryOnFailure(options.Retries.Value);

                if (options.Timeout is not null) builder.CommandTimeout(options.Timeout.Value.Seconds);
            }));

        if (!options.MigrateDb) return;

        using var sp = services.BuildServiceProvider();
        using var dbContext = sp.GetRequiredService<IDbContextFactory<TDbContext>>().CreateDbContext();
        if (dbContext.Database.GetPendingMigrations().Any()) dbContext.Database.Migrate();
    }
}

/// <summary>
///     The <see cref="DbContextRegistrationOptions" /> record.
/// </summary>
[PublicAPI]
public sealed record DbContextRegistrationOptions
{
    /// <summary>
    ///     Indicates that migrations should be applied on service startup.
    /// </summary>
    public bool MigrateDb { get; init; }

    /// <summary>
    ///     The timeout. Set <c>null</c> if you want default.
    /// </summary>
    public TimeSpan? Timeout { get; init; }

    /// <summary>
    ///     The retries num. Set <c>null</c> if you want default.
    /// </summary>
    public byte? Retries { get; init; }

    /// <summary>
    ///     The database connection string.
    /// </summary>
    public string? ConnectionString { get; init; }
}