namespace MawDbMigrateTool.Models;

public class VideoRating
{
    public int VideoId { get; set; }
    public short UserId { get; set; }
    public short Score { get; set; }
}