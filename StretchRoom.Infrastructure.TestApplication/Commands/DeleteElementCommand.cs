using Microsoft.EntityFrameworkCore;
using StretchRoom.Infrastructure.Interfaces;
using StretchRoom.Infrastructure.TestApplication.DaL;

namespace StretchRoom.Infrastructure.TestApplication.Commands;

// Registration in DI example:
// services.AddScopedCommand<DeleteElementCommand, DeleteElementCommandContext, DeleteElementCommandResult>();

public record DeleteElementCommandContext(string Name);

public record DeleteElementCommandResult(bool Success);

public class DeleteElementCommand(IDbContextFactory<DataModelContext> contextFactory)
    : ICommand<DeleteElementCommandContext, DeleteElementCommandResult>
{
    public async Task<DeleteElementCommandResult> ExecuteAsync(DeleteElementCommandContext context,
        CancellationToken token = default)
    {
        await using var dbContext = await contextFactory.CreateDbContextAsync(token);
        var num = await dbContext.TestTable.Where(t => t.Name == context.Name).ExecuteDeleteAsync(token);
        var result = num > 0;
        return new DeleteElementCommandResult(result);
    }
}