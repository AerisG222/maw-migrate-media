using System.Diagnostics;

namespace MawMediaMigrate.Exif;

class ExifExporter
    : IExifExporter
{
    readonly Lock _lockObj = new();

    public async Task<IEnumerable<ExifResult>> ExportExifData(DirectoryInfo mediaRoot)
    {
        var result = new List<ExifResult>();
        var files = mediaRoot.EnumerateFiles("*", SearchOption.AllDirectories);

        await Parallel.ForEachAsync(files, async (file, token) =>
        {
            if (file.Directory!.Name != "src")
            {
                return;
            }

            var outfile = await Export(file.FullName);

            if (outfile != null)
            {
                lock (_lockObj)
                {
                    result.Add(new ExifResult
                    {
                        Src = file.FullName,
                        ExifFile = outfile
                    });
                }
            }
        });

        return result;
    }

    async Task<string?> Export(string mediaFile)
    {
        if (string.IsNullOrWhiteSpace(mediaFile))
        {
            throw new ArgumentException("The media file cannot be null or empty.", nameof(mediaFile));
        }

        var outfile = $"{mediaFile}.json";

        if (File.Exists(outfile))
        {
            return outfile;
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

        if (process.ExitCode == 0)
        {
            return outfile;
        }

        return null;
    }
}
