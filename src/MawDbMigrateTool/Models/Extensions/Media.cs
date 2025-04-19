using MawDbMigrateTool.Models.Source;
using MawDbMigrateTool.Models.Target;

namespace MawDbMigrateTool.Models.Extensions;

public static class MediaExtensions
{
    public static Media ToTarget(
        this Photo photo,
        Guid categoryId,
        Guid adminId
    ) {
        return new Media
        {
            Id = Guid.CreateVersion7(),
            CategoryId = categoryId,
            MediaTypeId = MediaType.Photo.Id,
            LocationId = null,
            LocationOverrideId = null,
            Created = photo.CreateDate,
            CreatedBy = adminId,
            Modified = DateTime.MinValue,
            ModifiedBy = adminId,

            LegacyId = photo.Id,
            LegacyCategoryId = photo.CategoryId
        };
    }

    public static Media ToTarget(
        this Video video,
        Guid categoryId,
        Guid adminId
    ) {
        return new Media
        {
            Id = Guid.CreateVersion7(),
            CategoryId = categoryId,
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