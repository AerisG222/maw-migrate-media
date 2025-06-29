namespace MawDbMigrate.Models.Source;

public class PhotoCategory
{
    public short Id { get; set; }
    public short Year { get; set; }
    public string? Name { get; set; }
    public short TeaserPhotoWidth { get; set; }
    public short TeaserPhotoHeight { get; set; }
    public string? TeaserPhotoPath { get; set; }
    public DateTime CreateDate { get; set; }
    public double GpsLatitude { get; set; }
    public double GpsLongitude { get; set; }
    public string? GpsLatitudeRefId { get; set; }
    public string? GpsLongitudeRefId { get; set; }
    public int PhotoCount { get; set; }
    public long TotalSizeXs { get; set; }
    public long TotalSizeXsSq { get; set; }
    public long TotalSizeSm { get; set; }
    public long TotalSizeMd { get; set; }
    public long TotalSizeLg { get; set; }
    public long TotalSizePrt { get; set; }
    public long TotalSizeSrc { get; set; }
    public int TeaserPhotoSize { get; set; }
    public short TeaserPhotoSqHeight { get; set; }
    public short TeaserPhotoSqWidth { get; set; }
    public string? TeaserPhotoSqPath { get; set; }
    public int TeaserPhotoSqSize { get; set; }

    public DateTime EffectiveDate =>
        CreateDate != DateTime.MinValue
            ? CreateDate
            : new DateTime(Year, 1, 1).AddSeconds(Id);
}
