namespace MawDbMigrateTool;

public class Exporter
{
    readonly DbReader _dbReader;
    readonly ImportFileWriter _importWriter;

    public Exporter(string dbConnString, string sqlFile)
    {
        _dbReader = new DbReader(dbConnString);
        _importWriter = new ImportFileWriter(sqlFile);
    }

    public async Task ExportDatabase()
    {
        var db = await _dbReader.LoadData();
        await _importWriter.WriteImportFile(db);
    }
}