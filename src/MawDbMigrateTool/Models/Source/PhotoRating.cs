namespace MawDbMigrateTool.Models.Source;

public class PhotoRating
{
    public int PhotoId { get; set; }
    public short UserId { get; set; }
    public short Score { get; set; }
}