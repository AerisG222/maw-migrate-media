namespace MawDbMigrateTool.Models.Target;

public class MediaFile
{
    public Guid MediaId { get; set; }
    public Guid MediaTypeId { get; set; }
    public Guid ScaleId { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public long Bytes { get; set; }
    public string Path { get; set; } = string.Empty;
}