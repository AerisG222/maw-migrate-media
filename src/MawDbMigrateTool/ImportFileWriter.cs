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
        await WriteMedia(writer, db.Media);
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
                INSERT INTO maw.category (
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
                INSERT INTO maw.category_role (
                    category_id,
                    role_id
                ) VALUES (
                    {SqlAsString(categoryRole.CategoryId)},
                    {SqlAsString(categoryRole.RoleId)}
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
                INSERT INTO maw.media (
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

    async Task WriteComments(StreamWriter writer, List<Comment> comments)
    {
        await WriteHeader(writer, "COMMENTS");

        foreach (var comment in comments)
        {
            await writer.WriteLineAsync(
                $"""
                INSERT INTO maw.comment (
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
                INSERT INTO maw.rating (
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
                INSERT INTO maw.user (
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
                INSERT INTO maw.role (
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
                INSERT INTO maw.user_role (
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
