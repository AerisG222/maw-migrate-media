using System.Diagnostics;

namespace MawMediaMigrate;

class ExifExporter
{
    public async Task ExportAsync(string mediaFile)
    {
        if (string.IsNullOrWhiteSpace(mediaFile))
        {
            throw new ArgumentException("The media file cannot be null or empty.", nameof(mediaFile));
        }

        if(File.Exists($"{mediaFile}.json"))
        {
            return;
        }

        var arguments = $"-json -quiet -groupHeadings -long -textOut \"%d%f.%e.json\" \"{mediaFile}\"";
        var psi = new ProcessStartInfo
        {
            FileName = "exiftool",
            Arguments = arguments,
            UseShellExecute = false,
            CreateNoWindow = true
        };

        using var process = new Process { StartInfo = psi };

        process.Start();

        await process.WaitForExitAsync();
    }
}