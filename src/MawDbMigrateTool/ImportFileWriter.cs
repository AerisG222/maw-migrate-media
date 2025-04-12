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

        await WriteEnding(writer);

        await writer.FlushAsync();
        await writer.DisposeAsync();
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
