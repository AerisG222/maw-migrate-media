namespace MawDbMigrate.Models.Extensions;

public static class UserExtensions
{
    public static Target.User ToTarget(this Source.User source)
    {
        return new Target.User
        {
            Id = Guid.CreateVersion7(),
            Name = source.Username,
            Created = DateTime.MinValue,
            Modified = DateTime.MinValue,
            Email = source.Email,
            EmailVerified = true,
            GivenName = source.FirstName,
            Surname = source.LastName
        };
    }
}