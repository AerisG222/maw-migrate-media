namespace MawDbMigrateTool.Models;

public class PhotoGpsOverride
{
    public int PhotoId { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public short UserId { get; set; }
    public DateTime UpdatedTime { get; set; }
    public bool HasBeenReverseGeocoded { get; set; }
}