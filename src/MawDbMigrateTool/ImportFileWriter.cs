using MawDbMigrateTool.Models.Target;

namespace MawDbMigrateTool;

public class ImportFileWriter
{
    readonly string _sqlFile;

    public ImportFileWriter(string sqlFile)
    {
        _sqlFile = sqlFile;
    }

    public async Task WriteImportFile(Db db)
    {
        using var writer = new StreamWriter(_sqlFile);

        await WriteBeginning(writer);

        await WriteUsers(writer, db.Users);
        await WriteRoles(writer, db.Roles);
        await WriteUserRoles(writer, db.UserRoles);

        await WriteCategories(writer, db.Categories);
        await WriteCategoryRoles(writer, db.CategoryRoles);
        await WriteLocations(writer, db.Locations);
        await WritePointsOfInterest(writer, db.PointsOfInterest);
        await WriteMedia(writer, db.Media);
        await WriteMediaFiles(writer, db.MediaFiles);
        await WriteCategoryMedia(writer, db.CategoryMedia);
        await WriteComments(writer, db.Comments);
        await WriteRatings(writer, db.Ratings);

        await WriteEnding(writer);

        await writer.FlushAsync();
        await writer.DisposeAsync();
    }

    async Task WriteCategories(StreamWriter writer, List<Category> categories)
    {
        await WriteHeader(writer, "CATEGORIES");

        foreach (var category in categories)
        {
            await writer.WriteLineAsync(
                $"""
                INSERT INTO media.category (
                    id,
                    name,
                    teaser_media_id,
                    effective_date,
                    created,
                    modified
                ) VALUES (
                    {SqlAsString(category.Id)},
                    {SqlString(category.Name)},
                    {SqlAsString(category.TeaserMediaId)},
                    {SqlString(category.EffectiveDate.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlString(category.Created.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlString(category.Modified.ToString("yyyy-MM-dd HH:mm:ss"))}
                );
                """);
        }
    }

    async Task WriteCategoryRoles(StreamWriter writer, List<CategoryRole> categoryRoles)
    {
        await WriteHeader(writer, "CATEGORY_ROLES");

        foreach (var categoryRole in categoryRoles)
        {
            await writer.WriteLineAsync(
                $"""
                INSERT INTO media.category_role (
                    category_id,
                    role_id
                ) VALUES (
                    {SqlAsString(categoryRole.CategoryId)},
                    {SqlAsString(categoryRole.RoleId)}
                );
                """);
        }
    }

    async Task WriteLocations(StreamWriter writer, List<Location> locations)
    {
        await WriteHeader(writer, "LOCATIONS");

        foreach (var location in locations)
        {
            await writer.WriteLineAsync(
                $"""
                INSERT INTO media.location (
                    id,
                    latitude,
                    longitude,
                    lookup_date,
                    formatted_address,
                    administrative_area_level_1,
                    administrative_area_level_2,
                    administrative_area_level_3,
                    country,
                    locality,
                    neighborhood,
                    sub_locality_level_1,
                    sub_locality_level_2,
                    postal_code,
                    postal_code_suffix,
                    premise,
                    route,
                    street_number,
                    sub_premise
                ) VALUES (
                    {SqlAsString(location.Id)},
                    {SqlNonString(location.Latitude)},
                    {SqlNonString(location.Longitude)},
                    {SqlString(location.LookupDate.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlString(location.FormattedAddress)},
                    {SqlString(location.AdministrativeAreaLevel1)},
                    {SqlString(location.AdministrativeAreaLevel2)},
                    {SqlString(location.AdministrativeAreaLevel3)},
                    {SqlString(location.Country)},
                    {SqlString(location.Locality)},
                    {SqlString(location.Neighborhood)},
                    {SqlString(location.SubLocalityLevel1)},
                    {SqlString(location.SubLocalityLevel2)},
                    {SqlString(location.PostalCode)},
                    {SqlString(location.PostalCodeSuffix)},
                    {SqlString(location.Premise)},
                    {SqlString(location.Route)},
                    {SqlString(location.StreetNumber)},
                    {SqlString(location.SubPremise)}
                );
                """);
        }
    }

    async Task WritePointsOfInterest(StreamWriter writer, List<PointOfInterest> pointsOfInterest)
    {
        await WriteHeader(writer, "POINTS_OF_INTEREST");

        foreach (var poi in pointsOfInterest)
        {
            await writer.WriteLineAsync(
                $"""
                INSERT INTO media.point_of_interest (
                    location_id,
                    type,
                    name
                ) VALUES (
                    {SqlAsString(poi.LocationId)},
                    {SqlString(poi.Type)},
                    {SqlString(poi.Name)}
                );
                """);
        }
    }

    async Task WriteMedia(StreamWriter writer, List<Media> media)
    {
        await WriteHeader(writer, "MEDIA");

        foreach (var mediaItem in media)
        {
            await writer.WriteLineAsync(
                $"""
                INSERT INTO media.media (
                    id,
                    category_id,
                    media_type_id,
                    location_id,
                    location_override_id,
                    created,
                    modified
                ) VALUES (
                    {SqlAsString(mediaItem.Id)},
                    {SqlAsString(mediaItem.CategoryId)},
                    {SqlAsString(mediaItem.MediaTypeId)},
                    {SqlAsString(mediaItem.LocationId)},
                    {SqlAsString(mediaItem.LocationOverrideId)},
                    {SqlString(mediaItem.Created.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlString(mediaItem.Modified.ToString("yyyy-MM-dd HH:mm:ss"))}
                );
                """);
        }
    }

    async Task WriteMediaFiles(StreamWriter writer, List<MediaFile> mediaFiles)
    {
        await WriteHeader(writer, "MEDIA FILES");

        foreach(var file in mediaFiles)
        {
            await writer.WriteLineAsync(
                $"""
                INSERT INTO media.media_file (
                    media_id,
                    media_type_id,
                    scale_id,
                    width,
                    height,
                    bytes,
                    path
                ) VALUES (
                    {SqlAsString(file.MediaId)},
                    {SqlAsString(file.MediaTypeId)},
                    {SqlAsString(file.ScaleId)},
                    {SqlNonString(file.Width)},
                    {SqlNonString(file.Height)},
                    {SqlNonString(file.Bytes)},
                    {SqlString(file.Path)}
                );
                """);
        }
    }

    async Task WriteCategoryMedia(StreamWriter writer, List<CategoryMedia> categoryMedias)
    {
        await WriteHeader(writer, "CATEGORY MEDIA");

        foreach (var categoryMedia in categoryMedias)
        {
            await writer.WriteLineAsync(
                $"""
                INSERT INTO media.category_media (
                    category_id,
                    media_id,
                    is_teaser,
                    created,
                    created_by,
                    modified,
                    modified_by
                ) VALUES (
                    {SqlAsString(categoryMedia.CategoryId)},
                    {SqlAsString(categoryMedia.MediaId)},
                    {SqlNonString(categoryMedia.IsTeaser)},
                    {SqlString(categoryMedia.Created.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlAsString(categoryMedia.CreatedBy)},
                    {SqlString(categoryMedia.Modified.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlAsString(categoryMedia.ModifiedBy)}
                );
                """);
        }
    }

    async Task WriteComments(StreamWriter writer, List<Comment> comments)
    {
        await WriteHeader(writer, "COMMENTS");

        foreach (var comment in comments)
        {
            await writer.WriteLineAsync(
                $"""
                INSERT INTO media.comment (
                    id,
                    media_id,
                    created_by,
                    created,
                    modified,
                    body
                ) VALUES (
                    {SqlAsString(comment.Id)},
                    {SqlAsString(comment.MediaId)},
                    {SqlAsString(comment.CreatedBy)},
                    {SqlString(comment.Created.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlString(comment.Modified.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlString(comment.Body)}
                );
                """);
        }
    }

    async Task WriteRatings(StreamWriter writer, List<Rating> ratings)
    {
        await WriteHeader(writer, "RATINGS");

        foreach (var rating in ratings)
        {
            await writer.WriteLineAsync(
                $"""
                INSERT INTO media.rating (
                    media_id,
                    created_by,
                    created,
                    modified,
                    rating
                ) VALUES (
                    {SqlAsString(rating.MediaId)},
                    {SqlAsString(rating.CreatedBy)},
                    {SqlString(rating.Created.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlString(rating.Modified.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlNonString(rating.Score)}
                );
                """);
        }
    }

    async Task WriteUsers(StreamWriter writer, List<User> users)
    {
        await WriteHeader(writer, "USERS");

        foreach (var user in users)
        {
            await writer.WriteLineAsync(
                $"""
                INSERT INTO media.user (
                    id,
                    created,
                    modified,
                    name,
                    email,
                    email_verified,
                    given_name,
                    surname,
                    picture_url
                ) VALUES (
                    {SqlAsString(user.Id)},
                    {SqlString(user.Created.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlString(user.Modified.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlString(user.Name)},
                    {SqlString(user.Email)},
                    {SqlNonString(user.EmailVerified)},
                    {SqlString(user.GivenName)},
                    {SqlString(user.Surname)},
                    {SqlString(user.PictureUrl)}
                );
                """);
        }

        await writer.WriteLineAsync();
    }

    async Task WriteRoles(StreamWriter writer, List<Role> roles)
    {
        await WriteHeader(writer, "ROLES");

        foreach (var role in roles)
        {
            await writer.WriteLineAsync(
                $"""
                INSERT INTO media.role (
                    id,
                    name
                ) VALUES (
                    {SqlAsString(role.Id)},
                    {SqlString(role.Name)}
                );
                """);
        }
    }

    async Task WriteUserRoles(StreamWriter writer, List<UserRole> userRoles)
    {
        await WriteHeader(writer, "USER_ROLES");

        foreach (var userRole in userRoles)
        {
            await writer.WriteLineAsync(
                $"""
                INSERT INTO media.user_role (
                    user_id,
                    role_id
                ) VALUES (
                    {SqlAsString(userRole.UserId)},
                    {SqlAsString(userRole.RoleId)}
                );
                """);
        }
    }

    async Task WriteBeginning(StreamWriter writer)
    {
        await writer.WriteLineAsync(
            $"""
            DO
            $$
            BEGIN
            """
        );
    }

    async Task WriteEnding(StreamWriter writer)
    {
        await writer.WriteLineAsync(
            $"""
            END
            $$

            \\q
            """
        );
    }

    async Task WriteHeader(StreamWriter writer, string title)
    {
        await writer.WriteLineAsync();
        await writer.WriteLineAsync($"-- {title} --");
    }

    string SqlString(string? val) => val switch
        {
            null => "NULL",
            "" => "NULL",
            _ => $"'{val.Replace("'", "''")}'"
        };

    string SqlAsString<T>(T val) => val switch
        {
            null => "NULL",
            _ => $"'{val}'"
        };

    string SqlNonString<T>(T val) => val switch
        {
            null => "NULL",
            _ => $"{val}"
        };
}
