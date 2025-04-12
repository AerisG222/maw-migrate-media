namespace MawDbMigrateTool.Models.Source;

public class PhotoPointOfInterest
{
    public int PhotoId { get; set; }
    public string PoiType { get; set; }
    public string PoiName { get; set; }
    public bool IsOverride { get; set; }
}
