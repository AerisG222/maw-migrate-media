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

    public async Task WriteResults(IEnumerable<MoveResult> moveSpecs, IEnumerable<ExifResult> exifResults, IEnumerable<ScaleResult> scaledFiles)
    {
        await WriteJson(moveSpecs, "move.json");
        await WriteJson(exifResults, "exif.json");
        await WriteJson(scaledFiles, "scale.json");
    }

    async Task WriteJson<T>(IEnumerable<T> items, string fileName)
    {
        var filePath = Path.Combine(_outdir.FullName, fileName);

        await using var stream = File.Create(fileName);
        await JsonSerializer.SerializeAsync(stream, items, _jsonOptions);
    }
}
