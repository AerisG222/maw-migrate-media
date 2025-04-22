namespace MawDbMigrateTool.Models.Target;

public class Comment
{
    public Guid Id { get; set; }
    public Guid MediaId { get; set; }
    public DateTime Created { get; set; }
    public Guid CreatedBy { get; set; }
    public DateTime Modified { get; set; }
    public string? Body { get; set; }
}