namespace MawDbMigrateTool.Models.Target;

public class PointOfInterest
{
    public Guid LocationId { get; set; }
    public string? Type { get; set; }
    public string? Name { get; set; }
}
