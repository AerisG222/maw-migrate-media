using MawMediaMigrate.Results;

class MediaInfo
{
    public required string DestinationSrcPath { get; init; }
    public string? OriginalSrcPath { get; set; }
    public string? ExifFile { get; set; }
    public float? Duration { get; set; }
    public IEnumerable<ScaledFile> ScaledFiles { get; set; } = [];
}
