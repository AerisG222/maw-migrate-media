using MawDbMigrateTool.Models.Source;
using MawDbMigrateTool.Models.Target;

namespace MawDbMigrateTool.Models.Extensions;

public static class CategoryExtensions
{
    public static Category ToTarget(
        this PhotoCategory category,
        Guid userId
    ) {
        return new Category
        {
            Id = Guid.CreateVersion7(),
            Name = category.Name,
            TeaserMediaId = Guid.Empty,
            EffectiveDate = category.CreateDate,   // ?? TODO ??
            Created = category.CreateDate,
            CreatedBy = userId,
            Modified = DateTime.MinValue,
            ModifiedBy = userId,

            LegacyId = category.Id,
            LegacyType = 'p'
        };
    }

    public static Category ToTarget(
        this VideoCategory category,
        Guid userId
    ) {
        return new Category
        {
            Id = Guid.CreateVersion7(),
            Name = category.Name,
            TeaserMediaId = Guid.Empty,
            EffectiveDate = category.CreateDate,  //  ?? TODO ??
            Created = category.CreateDate,
            CreatedBy = userId,
            Modified = DateTime.MinValue,
            ModifiedBy = userId,

            LegacyId = category.Id,
            LegacyType = 'v'
        };
    }
}