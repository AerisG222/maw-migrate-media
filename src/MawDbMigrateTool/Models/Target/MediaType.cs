namespace MawDbMigrateTool.Models.Target;

public class MediaType
{
    public static readonly MediaType Photo = new()
    {
        Id = Guid.CreateVersion7(),
        Name = "photo"
    };

    public static readonly MediaType Video = new()
    {
        Id = Guid.CreateVersion7(),
        Name = "video"
    };

    public Guid Id { get; set; }
    public string? Name { get; set; }
}