using MawDbMigrateTool.Models.Target;

namespace MawDbMigrateTool;

public class ImportFileWriter
{
    readonly string _dir;

    public ImportFileWriter(string outDir)
    {
        _dir = outDir;
    }

    public async Task WriteImportFiles(Db db)
    {
        PrepareOutputDirectory();

        await WriteSqlScript("users.sql", async (writer) => await WriteUsers(writer, db.Users));
        await WriteSqlScript("roles.sql", async (writer) => await WriteRoles(writer, db.Roles));
        await WriteSqlScript("user-roles.sql", async (writer) => await WriteUserRoles(writer, db.UserRoles));

        await WriteSqlScript("categories.sql", async (writer) => await WriteCategories(writer, db.Categories));
        await WriteSqlScript("category-roles.sql", async (writer) => await WriteCategoryRoles(writer, db.CategoryRoles));
        await WriteSqlScript("locations.sql", async (writer) => await WriteLocations(writer, db.Locations));
        await WriteSqlScript("points-of-interest.sql", async (writer) => await WritePointsOfInterest(writer, db.PointsOfInterest));
        await WriteSqlScript("media.sql", async (writer) => await WriteMedia(writer, db.Media));
        await WriteSqlScript("media-files.sql", async (writer) => await WriteMediaFiles(writer, db.MediaFiles));
        await WriteSqlScript("category-media.sql", async (writer) => await WriteCategoryMedia(writer, db.CategoryMedia));
        await WriteSqlScript("comments.sql", async (writer) => await WriteComments(writer, db.Comments));
        await WriteSqlScript("ratings.sql", async (writer) => await WriteRatings(writer, db.Ratings));
    }

    void PrepareOutputDirectory()
    {
        if (!Directory.Exists(_dir))
        {
            Directory.CreateDirectory(_dir);
        }
    }

    async Task WriteSqlScript(string filename, Func<StreamWriter, Task> writeAction)
    {
        var outfile = Path.Combine(_dir, filename);
        using var writer = new StreamWriter(outfile);

        await WriteBeginning(writer);
        await writeAction(writer);
        await WriteEnding(writer);

        await writer.FlushAsync();
        await writer.DisposeAsync();
    }

    async Task WriteCategories(StreamWriter writer, List<Category> categories)
    {
        Console.WriteLine($"- writing {categories.Count} categories");
        await WriteHeader(writer, $"CATEGORIES ({categories.Count})");

        foreach (var category in categories)
        {
            await writer.WriteLineAsync(
                $"""
                INSERT INTO media.category (
                    id,
                    name,
                    effective_date,
                    created,
                    created_by,
                    modified,
                    modified_by
                ) VALUES (
                    {SqlAsString(category.Id)},
                    {SqlString(category.Name)},
                    {SqlString(category.EffectiveDate.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlString(category.Created.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlAsString(category.CreatedBy)},
                    {SqlString(category.Modified.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlAsString(category.ModifiedBy)}
                );
                """);
        }
    }

    async Task WriteCategoryRoles(StreamWriter writer, List<CategoryRole> categoryRoles)
    {
        Console.WriteLine($"- writing {categoryRoles.Count} category roles");
        await WriteHeader(writer, $"CATEGORY_ROLES ({categoryRoles.Count})");

        foreach (var categoryRole in categoryRoles)
        {
            await writer.WriteLineAsync(
                $"""
                INSERT INTO media.category_role (
                    category_id,
                    role_id,
                    created,
                    created_by
                ) VALUES (
                    {SqlAsString(categoryRole.CategoryId)},
                    {SqlAsString(categoryRole.RoleId)},
                    {SqlString(categoryRole.Created.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlAsString(categoryRole.CreatedBy)}
                );
                """);
        }
    }

    async Task WriteLocations(StreamWriter writer, List<Location> locations)
    {
        Console.WriteLine($"- writing {locations.Count} locations");
        await WriteHeader(writer, $"LOCATIONS ({locations.Count})");

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
        Console.WriteLine($"- writing {pointsOfInterest.Count} points of interest");
        await WriteHeader(writer, $"POINTS_OF_INTEREST ({pointsOfInterest.Count})");

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
        Console.WriteLine($"- writing {media.Count} media");
        await WriteHeader(writer, $"MEDIA ({media.Count})");

        foreach (var mediaItem in media)
        {
            await writer.WriteLineAsync(
                $"""
                INSERT INTO media.media (
                    id,
                    media_type_id,
                    location_id,
                    location_override_id,
                    created,
                    created_by,
                    modified,
                    modified_by
                ) VALUES (
                    {SqlAsString(mediaItem.Id)},
                    {SqlAsString(mediaItem.MediaTypeId)},
                    {SqlAsString(mediaItem.LocationId)},
                    {SqlAsString(mediaItem.LocationOverrideId)},
                    {SqlString(mediaItem.Created.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlAsString(mediaItem.CreatedBy)},
                    {SqlString(mediaItem.Modified.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlAsString(mediaItem.ModifiedBy)}
                );
                """);
        }
    }

    async Task WriteMediaFiles(StreamWriter writer, List<MediaFile> mediaFiles)
    {
        Console.WriteLine($"- writing {mediaFiles.Count} media files");
        await WriteHeader(writer, $"MEDIA FILES ({mediaFiles.Count})");

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
        Console.WriteLine($"- writing {categoryMedias.Count} category media");
        await WriteHeader(writer, $"CATEGORY MEDIA ({categoryMedias.Count})");

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
        Console.WriteLine($"- writing {comments.Count} comments");
        await WriteHeader(writer, $"COMMENTS ({comments.Count})");

        foreach (var comment in comments)
        {
            await writer.WriteLineAsync(
                $"""
                INSERT INTO media.comment (
                    id,
                    media_id,
                    created,
                    created_by,
                    modified,
                    body
                ) VALUES (
                    {SqlAsString(comment.Id)},
                    {SqlAsString(comment.MediaId)},
                    {SqlString(comment.Created.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlAsString(comment.CreatedBy)},
                    {SqlString(comment.Modified.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlString(comment.Body)}
                );
                """);
        }
    }

    async Task WriteRatings(StreamWriter writer, List<Rating> ratings)
    {
        Console.WriteLine($"- writing {ratings.Count} ratings");
        await WriteHeader(writer, $"RATINGS ({ratings.Count})");

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
        Console.WriteLine($"- writing {users.Count} users");
        await WriteHeader(writer, $"USERS ({users.Count})");

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
        Console.WriteLine($"- writing {roles.Count} roles");
        await WriteHeader(writer, $"ROLES ({roles.Count})");

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
        Console.WriteLine($"- writing {userRoles.Count} user roles");
        await WriteHeader(writer, $"USER_ROLES ({userRoles.Count})");

        foreach (var userRole in userRoles)
        {
            await writer.WriteLineAsync(
                $"""
                INSERT INTO media.user_role (
                    user_id,
                    role_id,
                    created,
                    created_by
                ) VALUES (
                    {SqlAsString(userRole.UserId)},
                    {SqlAsString(userRole.RoleId)},
                    {SqlString(userRole.Created.ToString("yyyy-MM-dd HH:mm:ss"))},
                    {SqlAsString(userRole.CreatedBy)}
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
