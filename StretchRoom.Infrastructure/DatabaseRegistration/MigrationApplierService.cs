using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StretchRoom.Infrastructure.Services.ExecutedServices;

namespace StretchRoom.Infrastructure.DatabaseRegistration;

/// <summary>
///     The <see cref="MigrationApplierService{TDbContext}" /> class.
/// </summary>
/// <param name="opts">The db context registration options.</param>
/// <param name="dbContextFactory">The db context factory.</param>
/// <typeparam name="TDbContext">The db context to migrate.</typeparam>
internal class MigrationApplierService<TDbContext>(
    IOptions<DbContextRegistrationOptions> opts,
    IDbContextFactory<TDbContext> dbContextFactory) : IBeforeHostingStartedService
    where TDbContext : DbContext
{
    /// <summary>
    ///     Applies the migrations.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns></returns>
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!opts.Value.MigrateDb) return;
        await using var dbContext = await dbContextFactory.CreateDbContextAsync(cancellationToken);
        var migrations = await dbContext.Database.GetPendingMigrationsAsync(cancellationToken);
        if (migrations.Any()) await dbContext.Database.MigrateAsync(cancellationToken);
    }
}