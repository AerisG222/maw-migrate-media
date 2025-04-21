namespace MawDbMigrateTool.Models.Target;

public class Scale
{
    public static readonly Scale Src = new()
    {
        Id = Guid.Parse("01965307-20f9-7e01-955e-be53d1786828"),
        Code = "src"
    };

    public Guid Id { get; set; }
    public string? Code { get; set; }
}