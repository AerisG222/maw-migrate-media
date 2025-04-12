namespace MawDbMigrateTool;

public class Exporter
{
    readonly DbReader _dbReader;
    readonly TargetBuilder _targetBuilder;
    readonly ImportFileWriter _importWriter;

    public Exporter(string dbConnString, string sqlFile)
    {
        _dbReader = new DbReader(dbConnString);
        _targetBuilder = new TargetBuilder();
        _importWriter = new ImportFileWriter(sqlFile);
    }

    public async Task ExportDatabase()
    {
        var src = await _dbReader.LoadData();
        var target = _targetBuilder.Build(src);
        await _importWriter.WriteImportFile(target);
    }
}