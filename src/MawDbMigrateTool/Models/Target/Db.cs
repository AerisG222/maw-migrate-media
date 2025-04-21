namespace MawDbMigrateTool.Models.Target;

public class Db
{
    public List<User> Users { get; } = [];
    public List<Role> Roles { get; } = [];
    public List<UserRole> UserRoles { get; } = [];

    public List<Category> Categories { get; } = [];
    public List<CategoryRole> CategoryRoles { get; } = [];
    public List<CategoryMedia> CategoryMedia { get; } = [];
    public List<Media> Media { get; } = [];
    public List<MediaFile> MediaFiles { get; } = [];
    public List<Comment> Comments { get; } = [];
    public List<Rating> Ratings { get; } = [];
    public List<Location> Locations { get; } = [];
    public List<PointOfInterest> PointsOfInterest { get; } = [];
}
