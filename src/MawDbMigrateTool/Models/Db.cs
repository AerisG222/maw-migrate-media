namespace MawDbMigrateTool.Models;

public class Db
{
    // USERS
    public IEnumerable<User> Users { get; set; } = [];
    public IEnumerable<Role> Roles { get; set; } = [];
    public IEnumerable<UserRole> UserRoles { get; set; } = [];

    // PHOTOS
    public IEnumerable<Photo> Photos { get; set; } = [];
    public IEnumerable<PhotoCategory> PhotoCategories { get; set; } = [];
    public IEnumerable<PhotoCategoryRole> PhotoCategoryRoles { get; set; } = [];
    public IEnumerable<PhotoComment> PhotoComments { get; set; } = [];
    public IEnumerable<PhotoGpsOverride> PhotoGpsOverrides { get; set; } = [];
    public IEnumerable<PhotoPointOfInterest> PhotoPointOfInterests { get; set; } = [];
    public IEnumerable<PhotoRating> PhotoRatings { get; set; } = [];
    public IEnumerable<PhotoReverseGeocode> PhotoReverseGeocodes { get; set; } = [];

    // VIDEOS
    public IEnumerable<Video> Videos { get; set; } = [];
    public IEnumerable<VideoCategory> VideoCategories { get; set; } = [];
    public IEnumerable<VideoCategoryRole> VideoCategoryRoles { get; set; } = [];
    public IEnumerable<VideoComment> VideoComments { get; set; } = [];
    public IEnumerable<VideoGpsOverride> VideoGpsOverrides { get; set; } = [];
    public IEnumerable<VideoPointOfInterest> VideoPointOfInterests { get; set; } = [];
    public IEnumerable<VideoRating> VideoRatings { get; set; } = [];
    public IEnumerable<VideoReverseGeocode> VideoReverseGeocodes { get; set; } = [];
}
