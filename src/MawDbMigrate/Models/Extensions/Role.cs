namespace MawDbMigrate.Models.Extensions;

public static class RoleExtensions
{
    public static Target.Role ToTarget(
        this Source.Role source,
        Guid adminId
    )
    {
        return new Target.Role
        {
            Id = Guid.CreateVersion7(),
            Name = source.Name,
            Created = DateTime.MinValue,
            CreatedBy = adminId,
        };
    }
}
