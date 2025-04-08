namespace MawDbMigrateTool.Models;

public class VideoPointOfInterest
{
    public int VideoId { get; set; }
    public string PoiType { get; set; }
    public string PoiName { get; set; }
    public bool IsOverride { get; set; }
}
