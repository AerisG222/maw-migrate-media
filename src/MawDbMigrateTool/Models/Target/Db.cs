namespace MawDbMigrateTool.Models.Target;

public class Db
{
    // USERS
    public List<User> Users { get; set; } = [];
    public List<Role> Roles { get; set; } = [];
    public List<UserRole> UserRoles { get; set; } = [];
}
