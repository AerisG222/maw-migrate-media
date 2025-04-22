namespace MawDbMigrateTool.Models.Target;

public class Media
{
    public Guid Id { get; set; }
    public Guid MediaTypeId { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? LocationOverrideId { get; set; }
    public DateTime Created { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime Modified { get; set; }
    public Guid ModifiedBy { get; set; }

    public int LegacyId { get; set; }
    public short LegacyCategoryId { get; set; }
}
