namespace MawMediaMigrate;

public class MoveSpec
{
    public required string Src { get; set; }
    public required string Dst { get; set; }
    public string? ExifFile { get; set; }
    public List<ScaledFile> ScaledFiles { get; } = [];
}