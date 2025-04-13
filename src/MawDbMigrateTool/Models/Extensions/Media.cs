using MawDbMigrateTool.Models.Source;
using MawDbMigrateTool.Models.Target;

namespace MawDbMigrateTool.Models.Extensions;

public static class MediaExtensions
{
    public static Media ToTarget(
        this Photo photo,
        Guid categoryId,
        Guid locationId,
        Guid locationOverrideId
    ) {
        return new Media
        {
            Id = Guid.CreateVersion7(),
            CategoryId = categoryId,
            MediaTypeId = MediaType.Photo.Id,
            LocationId = locationId,
            LocationOverrideId = locationOverrideId,
            Created = DateTime.MinValue,
            CreatedBy = Guid.Empty,
            Modified = DateTime.MinValue,
            ModifiedBy = Guid.Empty,

            LegacyId = photo.Id,
            LegacyCategoryId = photo.CategoryId
        };
    }

    public static Media ToTarget(
        this Video video,
        Guid categoryId,
        Guid locationId,
        Guid locationOverrideId
    ) {
        return new Media
        {
            Id = Guid.CreateVersion7(),
            CategoryId = categoryId,
            MediaTypeId = MediaType.Video.Id,
            LocationId = locationId,
            LocationOverrideId = locationOverrideId,
            Created = DateTime.MinValue,
            CreatedBy = Guid.Empty,
            Modified = DateTime.MinValue,
            ModifiedBy = Guid.Empty,

            LegacyId = video.Id,
            LegacyCategoryId = video.CategoryId
        };
    }
}