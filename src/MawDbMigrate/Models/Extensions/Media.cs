using MawDbMigrate.Models.Source;
using MawDbMigrate.Models.Target;

namespace MawDbMigrate.Models.Extensions;

public static class MediaExtensions
{
    public static Media ToTarget(
        this Photo photo,
        Guid adminId
    ) {
        return new Media
        {
            Id = Guid.CreateVersion7(),
            MediaTypeId = MediaType.Photo.Id,
            LocationId = null,
            LocationOverrideId = null,
            Created = photo.CreateDate ?? DateTime.MinValue,
            CreatedBy = adminId,
            Modified = DateTime.MinValue,
            ModifiedBy = adminId,

            LegacyId = photo.Id,
            LegacyCategoryId = photo.CategoryId
        };
    }

    public static Media ToTarget(
        this Video video,
        Guid adminId
    ) {
        return new Media
        {
            Id = Guid.CreateVersion7(),
            MediaTypeId = MediaType.Video.Id,
            LocationId = null,
            LocationOverrideId = null,
            Created = video.CreateDate,
            CreatedBy = adminId,
            Modified = DateTime.MinValue,
            ModifiedBy = adminId,

            LegacyId = video.Id,
            LegacyCategoryId = video.CategoryId
        };
    }
}
