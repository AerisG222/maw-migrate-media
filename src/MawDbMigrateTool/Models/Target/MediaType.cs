namespace MawDbMigrateTool.Models.Target;

public class MediaType
{
    public static readonly MediaType Photo = new()
    {
        Id = Guid.Parse("01964f94-fa50-7846-b2e6-26d4609cc972"),
        Name = "photo"
    };

    public static readonly MediaType Video = new()
    {
        Id = Guid.Parse("01964f94-fa51-705b-b0e2-b4c668ac6fab"),
        Name = "video"
    };

    public Guid Id { get; set; }
    public string? Name { get; set; }
}