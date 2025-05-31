using System.Text.Json;

namespace MawMediaMigrate.Results.Reader;

public class ResultReader
    : IResultReader
{
    public async Task<(IEnumerable<MoveResult> moveSpecs, IEnumerable<ExifResult> exifResults, IEnumerable<ScaleResult> scaledFiles)> ReadResults(DirectoryInfo srcDir)
    {
        if (!srcDir.Exists)
        {
            throw new DirectoryNotFoundException($"Source directory '{srcDir.FullName}' does not exist.");
        }

        var moveSpecs = await ReadJsonFile<MoveResult>(srcDir, "move.json");
        var exifResults = await ReadJsonFile<ExifResult>(srcDir, "exif.json");
        var scaledFiles = await ReadJsonFile<ScaleResult>(srcDir, "scale.json");

        return (moveSpecs, exifResults, scaledFiles);
    }

    private static async Task<IEnumerable<T>> ReadJsonFile<T>(DirectoryInfo srcDir, string fileName)
    {
        var filePath = Path.Combine(srcDir.FullName, fileName);

        if (!File.Exists(filePath))
        {
            return [];
        }

        var jsonContent = await File.ReadAllTextAsync(filePath);

        return JsonSerializer.Deserialize<IEnumerable<T>>(jsonContent) ?? [];
    }
}
