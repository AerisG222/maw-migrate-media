using System.Diagnostics;
using System.Text.Json;

namespace MawFileMover;

public class Inspector
{
    public async Task<(int width, int height)> QueryDimensions(string path)
    {
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
