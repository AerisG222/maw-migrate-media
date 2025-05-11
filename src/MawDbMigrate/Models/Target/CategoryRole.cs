namespace MawDbMigrate.Models.Target;

public class CategoryRole
{
    public Guid CategoryId { get; set; }
    public Guid RoleId { get; set; }
    public DateTime Created { get; set; }
    public Guid CreatedBy { get; set; }
}