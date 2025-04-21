namespace MawDbMigrateTool.Models.Target;

public class CategoryMedia
{
    public Guid CategoryId { get; set; }
    public Guid MediaId { get; set; }
    public bool IsTeaser { get; set; }
    public DateTime Created { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime Modified { get; set; }
    public Guid ModifiedBy { get; set; }
}
