using System.Text.Json;

namespace MawMediaMigrate.Results.Reader;

public class ResultReader
    : IResultReader
{
    public async Task<(
        IEnumerable<MoveResult> moveSpecs,
        IEnumerable<ExifResult> exifResults,
        IEnumerable<ScaleResult> scaledFiles,
        IEnumerable<DurationResult> durationResults
    )> ReadResults(DirectoryInfo srcDir)
    {
        if (!srcDir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory '{srcDir.FullName}' does not exist.");
        }

        var moveSpecs = await ReadJsonFile<MoveResult>(srcDir, "move.json");
        var exifResults = await ReadJsonFile<ExifResult>(srcDir, "exif.json");
        var scaledFiles = await ReadJsonFile<ScaleResult>(srcDir, "scale.json");
        var durationResults = await ReadJsonFile<DurationResult>(srcDir, "duration.json");

        return (moveSpecs, exifResults, scaledFiles, durationResults);
    }

    private static Task<IEnumerable<T>> ReadJsonFile<T>(DirectoryInfo srcDir, string fileName)
    {
        var filePath = Path.Combine(srcDir.FullName, fileName);

        if (!File.Exists(filePath))
        {
            var res = Array.Empty<T>();

            return Task.FromResult(res.AsEnumerable());
        }

        using var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        var data = JsonSerializer.Deserialize<IEnumerable<T>>(fs) ?? [];

        return Task.FromResult(data);
    }
}
