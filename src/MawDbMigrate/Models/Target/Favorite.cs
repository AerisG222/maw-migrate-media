namespace MawDbMigrate.Models.Target;

public class Favorite
{
    public Guid MediaId { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime Created { get; set; }
}
