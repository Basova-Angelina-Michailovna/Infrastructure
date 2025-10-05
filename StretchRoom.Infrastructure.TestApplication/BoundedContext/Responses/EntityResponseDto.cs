using StretchRoom.Infrastructure.TestApplication.DaL;

namespace StretchRoom.Infrastructure.TestApplication.BoundedContext.Responses;

public record EntityResponseDto(int Id, string Name)
{
    public static EntityResponseDto CreateFromDbo(TestTableDbo dbo)
    {
        return new EntityResponseDto(dbo.Id, dbo.Name);
    }
}