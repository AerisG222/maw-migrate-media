namespace MawDbMigrateTool.Models.Source;

public class PhotoComment
{
    public int Id { get; set; }
    public int PhotoId { get; set; }
    public short UserId { get; set; }
    public DateTime EntryDate { get; set; }
    public string Message { get; set; }
}