using MawDbMigrateTool.Models;

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

        await writer.WriteLineAsync("hey");

        await writer.FlushAsync();
        await writer.DisposeAsync();
    }
}
