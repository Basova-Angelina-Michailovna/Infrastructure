using Microsoft.EntityFrameworkCore;
using StretchRoom.Infrastructure.Interfaces;
using StretchRoom.Infrastructure.TestApplication.DaL;

namespace StretchRoom.Infrastructure.TestApplication.Commands;

public record AddEntityCommandContext(string Name);

public record AddEntityCommandResult(int Id, string Name);

public class AddEntityCommand(IDbContextFactory<DataModelContext> dbFactory)
    : ICommand<AddEntityCommandContext, AddEntityCommandResult>
{
    public async Task<AddEntityCommandResult> ExecuteAsync(AddEntityCommandContext context,
        CancellationToken token = default)
    {
        await using var dbContext = await dbFactory.CreateDbContextAsync(token);

        var entry = await dbContext.TestTable.AddAsync(new TestTableDbo
        {
            Name = context.Name
        }, token);

        await dbContext.SaveChangesAsync(token);

        return new AddEntityCommandResult(entry.Entity.Id, entry.Entity.Name);
    }
}