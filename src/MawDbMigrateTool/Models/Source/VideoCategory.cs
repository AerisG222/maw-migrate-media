namespace MawDbMigrateTool.Models.Source;

public class VideoCategory
{
    public short Id { get; set; }
    public short Year { get; set; }
    public string? Name { get; set; }
    public short TeaserImageWidth { get; set; }
    public short TeaserImageHeight { get; set; }
    public string TeaserImagePath { get; set; }
    public DateTime CreateDate { get; set; }
    public double GpsLatitude { get; set; }
    public double GpsLongitude { get; set; }
    public string GpsLatitudeRefId { get; set; }
    public string GpsLongitudeRefId { get; set; }
    public int VideoCount { get; set; }
    public int TotalDuration { get; set; }
    public long TotalSizeThumb { get; set; }
    public long TotalSizeThumbSq { get; set; }
    public long TotalSizeScaled { get; set; }
    public long TotalSizeFull { get; set; }
    public long TotalSizeRaw { get; set; }
    public int TeaserImageSize { get; set; }
    public short TeaserImageSqHeight { get; set; }
    public short TeaserImageSqWidth { get; set; }
    public string TeaserImageSqPath { get; set; }
    public int TeaserImageSqSize { get; set; }

    public DateTime SortKey =>
        CreateDate != DateTime.MinValue
            ? CreateDate
            : new DateTime(Year, 1, 1, 0, 0, 0, Id, DateTimeKind.Utc);
}
