namespace MawDbMigrate.Models.Source;

public class Photo
{
    public int Id { get; set; }
    public short CategoryId { get; set; }
    public short XsHeight { get; set; }
    public short XsWidth { get; set; }
    public string? XsPath { get; set; }
    public short SmHeight { get; set; }
    public short SmWidth { get; set; }
    public string? SmPath { get; set; }
    public short MdHeight { get; set; }
    public short MdWidth { get; set; }
    public string? MdPath { get; set; }
    public short LgHeight { get; set; }
    public short LgWidth { get; set; }
    public string? LgPath { get; set; }
    public short PrtHeight { get; set; }
    public short PrtWidth { get; set; }
    public string? PrtPath { get; set; }
    public short SrcHeight { get; set; }
    public short SrcWidth { get; set; }
    public string? SrcPath { get; set; }
    public DateTime CreateDate { get; set; }

    public short AwsGlacierVaultId { get; set; }
    public string? AwsArchiveId { get; set; }
    public string? AwsTreehash { get; set; }
    public int XsSize { get; set; }
    public int SmSize { get; set; }
    public int MdSize { get; set; }
    public int LgSize { get; set; }
    public int PrtSize { get; set; }
    public int SrcSize { get; set; }
    public short XsSqHeight { get; set; }
    public short XsSqWidth { get; set; }
    public string? XsSqPath { get; set; }
    public int XsSqSize { get; set; }

    public double? GpsLatitude { get; set; }
    public double? GpsLongitude { get; set; }
}
