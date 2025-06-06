namespace MawDbMigrate.Models.Target;

public class Role
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public DateTime Created { get; set; }
    public Guid CreatedBy { get; set; }
}
