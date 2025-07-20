using System.Text.Json;
using CliWrap;
using CliWrap.Buffered;
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

        return await PrepareJson(file, outfile);
    }

    async Task<ExifResult> PrepareJson(FileInfo file, FileInfo outfile)
    {
        await ExportExifAsJson(file);
        await FormatJson(outfile);

        return new ExifResult
        {
            Src = file.FullName,
            ExifFile = outfile.FullName
        };
    }

    async Task ExportExifAsJson(FileInfo file)
    {
        await Cli
            .Wrap("exiftool")
            .WithArguments([
                "-json",
                "-quiet",
                "-groupHeadings",
                "-long",
                "-textOut",
                "%d%f.%e.json",
                file.FullName
            ])
            .ExecuteBufferedAsync();
    }

    async Task FormatJson(FileInfo outfile)
    {
        var exif = await GetExifData(outfile);

        await File.WriteAllTextAsync(outfile.FullName, exif.GetRawText());
    }

    async Task<JsonElement> GetExifData(FileInfo outfile)
    {
        using var fs = new FileStream(outfile.FullName, FileMode.Open, FileAccess.Read);
        using var json = await JsonDocument.ParseAsync(fs);

        if (json == null)
        {
            throw new InvalidOperationException($"Failed to parse json: {outfile.FullName}");
        }

        return json.RootElement[0].Clone();
    }
}
