namespace MawDbMigrateTool.Models.Target;

public class Category
{
    public Guid Id { get; set; }
    public string? Name { get; set; }
    public Guid TeaserMediaId { get; set; }
    public DateTime EffectiveDate { get; set; }
    public DateTime Created { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime Modified { get; set; }
    public Guid ModifiedBy { get; set; }

    public short LegacyId { get; set; }
    public char LegacyType { get; set; }
}
