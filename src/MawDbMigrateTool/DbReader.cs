using System.Data;
using Dapper;
using Npgsql;
using MawDbMigrateTool.Models;

namespace MawDbMigrateTool;

public class DbReader
{
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
        db.Users = await LoadTable<User>("maw.users", conn);
        db.UserRoles = await LoadTable<UserRole>("maw.user_role", conn);

        db.PhotoCategories = await LoadTable<PhotoCategory>("photo.category", conn);
        db.PhotoCategoryRoles = await LoadTable<PhotoCategoryRole>("photo.category_role", conn);
        db.PhotoComments = await LoadTable<PhotoComment>("photo.comment", conn);
        db.PhotoGpsOverrides = await LoadTable<PhotoGpsOverride>("photo.gps_override", conn);
        db.PhotoPointOfInterests = await LoadTable<PhotoPointOfInterest>("photo.point_of_interest", conn);
        db.PhotoRatings = await LoadTable<PhotoRating>("photo.rating", conn);
        db.PhotoReverseGeocodes = await LoadTable<PhotoReverseGeocode>("photo.reverse_geocode", conn);
        db.Photos = await LoadTable<Photo>("photo.photo", conn);

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

    public async Task<IEnumerable<T>> LoadTable<T>(string tableName, NpgsqlConnection conn)
    {
        return (await conn.QueryAsync<T>("SELECT * FROM " + tableName)).ToList();
    }
}