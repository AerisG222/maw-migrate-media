using System.Diagnostics;
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
        var result = await Cli.Wrap("exiftool")
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

    public async Task<(int width, int height)> QueryDimensions(string path)
    {
        if (_sourceInspectionResults.TryGetValue(path, out InspectResult? res))
        {
            return (res.ImageWidth, res.ImageHeight);
        }

        var psi = new ProcessStartInfo
        {
            FileName = "exiftool",
            Arguments = $"-j -ImageHeight -ImageWidth \"{path}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process
        {
            StartInfo = psi
        };

        process.Start();
        var data = await process.StandardOutput.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException($"exiftool exited with code: {process.ExitCode}");
        }

        if (data == null)
        {
            throw new InvalidDataException("Expected to get json from exiftool!");
        }

        var json = JsonDocument.Parse(data);
        var width = json.RootElement[0].GetProperty("ImageWidth").GetInt32();
        var height = json.RootElement[0].GetProperty("ImageHeight").GetInt32();

        return (width, height);
    }
}
