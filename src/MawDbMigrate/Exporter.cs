namespace MawDbMigrate;

public class Exporter
{
    readonly DbReader _dbReader;
    readonly TargetBuilder _targetBuilder;
    readonly ImportFileWriter _importWriter;

    public Exporter(string dbConnString, string outDir)
    {
        _dbReader = new DbReader(dbConnString);
        _targetBuilder = new TargetBuilder();
        _importWriter = new ImportFileWriter(outDir);
    }

    public async Task ExportDatabase()
    {
        var src = await _dbReader.LoadData();
        var target = _targetBuilder.Build(src);
        await _importWriter.WriteImportFiles(target);
    }
}