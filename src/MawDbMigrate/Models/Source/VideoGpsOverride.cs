namespace MawDbMigrate.Models.Source;

public class VideoGpsOverride
{
    public int VideoId { get; set; }
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public short UserId { get; set; }
    public DateTime UpdatedTime { get; set; }
    public bool HasBeenReverseGeocoded { get; set; }
}
