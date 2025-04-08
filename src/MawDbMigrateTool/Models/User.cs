namespace MawDbMigrateTool.Models;

public class User
{
    public short Id { get; set; }
    public string? Username { get; set; }
    public string? HashedPassword { get; set; }
    public string? SecurityStamp { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? EnableGithubAuth { get; set; }
    public string? EnableGoogleAuth { get; set; }
    public string? EnableMicrosoftAuth { get; set; }
    public string? EnableTwitterAuth { get; set; }
}
