class MediaInfo
{
    public required string DestinationSrcPath { get; init; }
    public string? OriginalSrcPath { get; set; }
    public string? ExifFile { get; set; }
    public IEnumerable<string> ScaledFiles { get; set; } = [];
}
