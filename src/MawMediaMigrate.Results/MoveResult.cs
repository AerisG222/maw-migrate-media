namespace MawMediaMigrate.Results;

public class MoveResult
{
    public required string Src { get; set; }
    public required string Dst { get; set; }
    public int Height { get; set; }
    public int Width { get; set; }
    public long Bytes { get; set; }
}
