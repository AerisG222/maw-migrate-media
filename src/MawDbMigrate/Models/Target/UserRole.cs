namespace MawDbMigrate.Models.Target;

public class UserRole
{
    public Guid UserId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime Created { get; set; }
    public Guid CreatedBy { get; set; }
}
