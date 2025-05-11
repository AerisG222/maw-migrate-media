namespace MawDbMigrate.Models.Target;

public class User
{
    public Guid Id { get; set; }
    public DateTime Created { get; set; }
    public DateTime Modified { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public bool EmailVerified { get; set; }
    public string? GivenName { get; set; }
    public string? Surname { get; set; }
    public string? PictureUrl { get; set; }
}
