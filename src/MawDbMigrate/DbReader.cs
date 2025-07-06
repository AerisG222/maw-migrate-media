using System.Data;
using Dapper;
using Npgsql;
using MawDbMigrate.Models.Source;

namespace MawDbMigrate;

public class DbReader
{
    const short _demoRoleId = 3;
    static readonly short[] _demoCategoryIds = [
        1400, // existing demo id: 1559,
        1447, // existing demo id: 1560,
        1477, // existing demo id: 1561,
        1500, // existing demo id: 1562,
        1546, // existing demo id: 1563
    ];

    readonly string _dbConnString;

    public DbReader(string dbConnString)
    {
        _dbConnString = dbConnString;
    }

    public async Task<Db> LoadData()
    {
        var db = new Db();

        using var conn = new NpgsqlConnection(_dbConnString);
        await conn.OpenAsync();
        var tran = await conn.BeginTransactionAsync(IsolationLevel.Serializable);

        db.Roles = await LoadTable<Role>("maw.role", conn);
        db.Users = await LoadTable<User>("maw.user", conn);
        db.UserRoles = await LoadTable<UserRole>("maw.user_role", conn);

        // for the google demo role, we ended up creating new categories that point to the same files
        // which causes issues for the generic loader - so for a few tables, we load via query to exclude those
        db.PhotoCategories = await LoadPhotoCategoryTable(conn);
        db.PhotoCategoryRoles = await LoadPhotoCategoryRolesTable(conn);
        db.PhotoComments = await LoadTable<PhotoComment>("photo.comment", conn);
        db.PhotoGpsOverrides = await LoadTable<PhotoGpsOverride>("photo.gps_override", conn);
        db.PhotoPointOfInterests = await LoadTable<PhotoPointOfInterest>("photo.point_of_interest", conn);
        db.PhotoRatings = await LoadTable<PhotoRating>("photo.rating", conn);
        db.PhotoReverseGeocodes = await LoadTable<PhotoReverseGeocode>("photo.reverse_geocode", conn);
        db.Photos = await LoadPhotoTable(conn);

        db.VideoCategories = await LoadTable<VideoCategory>("video.category", conn);
        db.VideoCategoryRoles = await LoadTable<VideoCategoryRole>("video.category_role", conn);
        db.VideoComments = await LoadTable<VideoComment>("video.comment", conn);
        db.VideoGpsOverrides = await LoadTable<VideoGpsOverride>("video.gps_override", conn);
        db.VideoPointOfInterests = await LoadTable<VideoPointOfInterest>("video.point_of_interest", conn);
        db.VideoRatings = await LoadTable<VideoRating>("video.rating", conn);
        db.VideoReverseGeocodes = await LoadTable<VideoReverseGeocode>("video.reverse_geocode", conn);
        db.Videos = await LoadTable<Video>("video.video", conn);

        await tran.RollbackAsync();
        await conn.CloseAsync();

        return db;
    }

    async Task<IEnumerable<T>> LoadTable<T>(string tableName, NpgsqlConnection conn)
    {
        Console.WriteLine($"- reading: {tableName}");

        var results = await conn.QueryAsync<T>("SELECT * FROM " + tableName);

        Console.WriteLine($"  - found: {results.Count()} rows");

        return results;
    }

    async Task<IEnumerable<PhotoCategory>> LoadPhotoCategoryTable(NpgsqlConnection conn)
    {
        Console.WriteLine("- reading: photo.photo_category");

        var results = await conn.QueryAsync<PhotoCategory>(
            """
            SELECT DISTINCT c.*
            FROM photo.category c
            INNER JOIN photo.category_role cr
                ON c.id = cr.category_id
                AND cr.role_id <> @roleId;
            """,
            new
            {
                roleId = _demoRoleId
            });

        Console.WriteLine($"  - found: {results.Count()} rows");

        return results;
    }

    async Task<IEnumerable<PhotoCategoryRole>> LoadPhotoCategoryRolesTable(NpgsqlConnection conn)
    {
        Console.WriteLine("- reading: photo.category_role");

        var results = (await conn.QueryAsync<PhotoCategoryRole>(
            """
            SELECT cr.*
            FROM photo.category_role cr
            INNER JOIN photo.category c
                ON c.id = cr.category_id
            WHERE cr.role_id <> @roleId
            """,
            new
            {
                roleId = _demoRoleId
            })
        ).ToList();

        foreach (var catId in _demoCategoryIds)
        {
            results.Add(new PhotoCategoryRole
            {
                CategoryId = catId,
                RoleId = _demoRoleId
            });
        }

        Console.WriteLine($"  - found: {results.Count()} rows");

        return results;
    }

    async Task<IEnumerable<Photo>> LoadPhotoTable(NpgsqlConnection conn)
    {
        Console.WriteLine("- reading: photo.photo");

        var results = await conn.QueryAsync<Photo>(
            """
            SELECT DISTINCT p.*
            FROM photo.photo p
            INNER JOIN photo.category c
                ON c.id = p.category_id
            INNER JOIN photo.category_role cr
                ON c.id = cr.category_id
            WHERE cr.role_id <> @roleId
            """,
            new
            {
                roleId = _demoRoleId
            });

        Console.WriteLine($"  - found: {results.Count()} rows");

        return results;
    }
}
