namespace MawDbMigrateTool.Models.Extensions;

public static class RoleExtensions
{
    public static Target.Role ToTarget(this Source.Role source)
    {
        return new Target.Role
        {
            Id = Guid.CreateVersion7(),
            Name = source.Name
        };
    }
}