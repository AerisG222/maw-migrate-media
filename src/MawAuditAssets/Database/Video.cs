namespace MawAuditAssets.Database;

public class Video
{
    public int Id { get; set; }
    public string? ThumbPath { get; set; }
    public string? ScaledPath { get; set; }
    public string? FullPath { get; set; }
    public string? RawPath { get; set; }
    public string? ThumbSqPath { get; set; }
}
