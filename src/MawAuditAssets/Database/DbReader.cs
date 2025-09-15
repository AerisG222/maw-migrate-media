using System.Data;
using Dapper;
using Npgsql;

[module: DapperAot]

namespace MawAuditAssets.Database;

public class DbReader
{
    readonly string _dbConnString;

    public DbReader(string dbConnString)
    {
        _dbConnString = dbConnString;

        DefaultTypeMap.MatchNamesWithUnderscores = true;
    }

    public async Task<IEnumerable<string>> LoadExpectedFilesFromDatabase()
    {
        using var conn = new NpgsqlConnection(_dbConnString);
        await conn.OpenAsync();
        var tran = await conn.BeginTransactionAsync(IsolationLevel.Serializable);

        var photos = await LoadTable<Photo>("photo.photo", conn);
        var videos = await LoadTable<Video>("video.video", conn);

        await tran.RollbackAsync();
        await conn.CloseAsync();

        var photoPaths = photos
            .SelectMany(p => new string?[] { p.LgPath, p.MdPath, p.PrtPath, p.SmPath, p.SrcPath, p.XsPath, p.XsSqPath })
            .Where(x => x != null)
            .Where(x => !x!.Contains("/src/"))
            .Cast<string>();

        var videoPaths = videos
            .SelectMany(v => new string?[] { v.FullPath, v.RawPath, v.ScaledPath, v.ThumbPath, v.ThumbSqPath })
            .Where(x => x != null)
            .Where(x => !x!.Contains("/raw/"))
            .Cast<string>();

        return photoPaths.Concat(videoPaths);
    }

    async Task<IEnumerable<T>> LoadTable<T>(string tableName, NpgsqlConnection conn)
    {
        Console.WriteLine($"- reading: {tableName}");

        var results = await conn.QueryAsync<T>("SELECT * FROM " + tableName);

        Console.WriteLine($"  - found: {results.Count()} rows");

        return results;
    }
}
