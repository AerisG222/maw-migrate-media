using System.Text.Json;
using CliWrap;
using CliWrap.Buffered;

namespace MawMediaMigrate.Scale;

class Inspector
    : IInspector
{
    readonly Dictionary<string, InspectResult> _sourceInspectionResults = [];

    public async Task BulkLoadSourceDimensions(string rootPath)
    {
        using var cmd = Cli
            .Wrap("exiftool")
            .WithArguments([
                "-j",
                "-r",
                "-i", "2k",
                "-i", "4k",
                "-i", "md",
                "-i", "sm",
                "-i", "src",
                "-i", "xs",
                "-i", "xs_sq",
                "-i", "full",
                "-i", "scaled",
                "-i", "thumbnails",
                "-i", "thumb_sq",
                "-ImageHeight",
                "-ImageWidth",
                rootPath
            ])
            .ExecuteBufferedAsync();

        var result = await cmd;

        if (string.IsNullOrWhiteSpace(result.StandardOutput))
        {
            throw new InvalidDataException("Expected to get json from exiftool!");
        }

        var results = JsonSerializer.Deserialize<InspectResult[]>(result.StandardOutput);

        if (results == null)
        {
            throw new Exception("Unable to bulk inspect source files!");
        }

        foreach (var r in results)
        {
            _sourceInspectionResults.Add(r.SourceFile, r);
        }
    }

    public async Task<InspectResult> QueryDimensions(string path)
    {
        if (_sourceInspectionResults.TryGetValue(path, out InspectResult? res))
        {
            return res;
        }

        using var cmd = Cli
            .Wrap("exiftool")
            .WithArguments([
                "-j",
                "-ImageHeight",
                "-ImageWidth",
                path
            ])
            .ExecuteBufferedAsync();

        var process = await cmd;

        return JsonSerializer.Deserialize<InspectResult[]>(process.StandardOutput)!.First();
    }
}
