using System.Text.Json;
using System.Text.Json.Serialization;

namespace MawMediaMigrate.Results.Writer;

public class ResultWriter
    : IResultWriter
{
    readonly DirectoryInfo _outdir;
    readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    public ResultWriter(DirectoryInfo outdir)
    {
        ArgumentNullException.ThrowIfNull(outdir);

        _outdir = outdir;
    }

    public async Task WriteResults<T>(IEnumerable<T> results, string filename)
    {
        var filePath = Path.Combine(_outdir.FullName, filename);

        await using var stream = File.Create(filePath);
        await JsonSerializer.SerializeAsync(stream, results, _jsonOptions);
    }
}
