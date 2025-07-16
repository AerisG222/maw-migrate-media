namespace MawDbMigrate.Models.Source;

public class Video
{
    public int Id { get; set; }
    public short CategoryId { get; set; }
    public short Duration { get; set; }
    public short ThumbHeight { get; set; }
    public short ThumbWidth { get; set; }
    public string? ThumbPath { get; set; }
    public short ScaledHeight { get; set; }
    public short ScaledWidth { get; set; }
    public string? ScaledPath { get; set; }
    public short FullHeight { get; set; }
    public short FullWidth { get; set; }
    public string? FullPath { get; set; }
    public string? RawPath { get; set; }
    public DateTime CreateDate { get; set; }
    public decimal? GpsLatitude { get; set; }
    public decimal? GpsLongitude { get; set; }
    public string? GpsLatitudeRefId { get; set; }
    public string? GpsLongitudeRefId { get; set; }
    public int ThumbSize { get; set; }
    public long ScaledSize { get; set; }
    public long FullSize { get; set; }
    public short RawHeight { get; set; }
    public short RawWidth { get; set; }
    public long RawSize { get; set; }
    public short ThumbSqHeight { get; set; }
    public short ThumbSqWidth { get; set; }
    public string? ThumbSqPath { get; set; }
    public int ThumbSqSize { get; set; }
    public short AwsGlacierVaultId { get; set; }
    public string? AwsArchiveId { get; set; }
    public string? AwsTreehash { get; set; }
}
