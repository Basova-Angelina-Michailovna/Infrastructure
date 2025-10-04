using FluentValidation;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StretchRoom.Infrastructure.Services.ExecutedServices;

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

        var validator = new DbContextRegistrationOptionsValidator();
        validator.ValidateAndThrow(options);

        services.AddOptions<DbContextRegistrationOptions>().Configure(registrationOptions);

        services.AddDbContextFactory<TDbContext>(OptionsAction);
        //services.AddDbContext<TDbContext>(OptionsAction);
        services.AddBeforeHostingStarted<MigrationApplierService<TDbContext>>();
        return;

        void OptionsAction(DbContextOptionsBuilder opts)
        {
            opts.UseNpgsql(options.ConnectionString, builder =>
            {
                if (options.Retries is not null) builder.EnableRetryOnFailure(options.Retries.Value);

                if (options.Timeout is not null) builder.CommandTimeout(options.Timeout.Value.Seconds);
            });
        }
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
    public bool MigrateDb { get; set; }

    /// <summary>
    ///     The timeout. Set <c>null</c> if you want default.
    /// </summary>
    public TimeSpan? Timeout { get; set; }

    /// <summary>
    ///     The retries num. Set <c>null</c> if you want default.
    /// </summary>
    public byte? Retries { get; set; }

    /// <summary>
    ///     The database connection string.
    /// </summary>
    public string? ConnectionString { get; set; }
}

internal class DbContextRegistrationOptionsValidator : AbstractValidator<DbContextRegistrationOptions>
{
    public DbContextRegistrationOptionsValidator()
    {
        RuleFor(it => it).NotNull();
        RuleFor(it => it.ConnectionString).NotEmpty();
    }
}