using MawDbMigrate.Models.Source;
using MawDbMigrate.Models.Target;

namespace MawDbMigrate.Models.Extensions;

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
            EffectiveDate = category.EffectiveDate,
            Created = category.EffectiveDate,
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
            EffectiveDate = category.EffectiveDate,
            Created = category.EffectiveDate,
            CreatedBy = userId,
            Modified = DateTime.MinValue,
            ModifiedBy = userId,

            LegacyId = category.Id,
            LegacyType = 'v'
        };
    }
}
