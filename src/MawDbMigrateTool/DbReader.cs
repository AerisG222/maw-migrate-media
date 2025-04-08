using MawDbMigrateTool.Models;
using Npgsql;

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

        await conn.CloseAsync();

        return db;
    }
}