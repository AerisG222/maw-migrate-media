using System.Diagnostics;
using MawMediaMigrate.Results;

namespace MawMediaMigrate.Exif;

class ExifExporter
    : IExifExporter
{
    public async Task<ExifResult> Export(FileInfo file, FileInfo outfile)
    {
        if (outfile.Exists)
        {
            return new ExifResult
            {
                Src = file.FullName,
                ExifFile = outfile.FullName
            };
        }

        var arguments = $"-json -quiet -groupHeadings -long -textOut \"%d%f.%e.json\" \"{file.FullName}\"";
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
            return new ExifResult
            {
                Src = file.FullName,
                ExifFile = outfile.FullName
            };
        }

        throw new InvalidOperationException(
            $"ExifTool failed to process file {file.FullName}. Exit code: {process.ExitCode}"
        );
    }
}
