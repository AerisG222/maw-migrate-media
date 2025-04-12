namespace MawDbMigrateTool.Models.Source;

public class VideoRating
{
    public int VideoId { get; set; }
    public short UserId { get; set; }
    public short Score { get; set; }
}