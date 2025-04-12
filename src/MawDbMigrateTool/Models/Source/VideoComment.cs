namespace MawDbMigrateTool.Models.Source;

public class VideoComment
{
    public int Id { get; set; }
    public int VideoId { get; set; }
    public short UserId { get; set; }
    public DateTime EntryDate { get; set; }
    public string Message { get; set; }
}