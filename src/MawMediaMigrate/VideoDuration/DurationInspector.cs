using CliWrap;
using CliWrap.Buffered;
using MawMediaMigrate.Results;

namespace MawMediaMigrate.VideoDuration;

class DurationInspector
{
    public async Task<DurationResult> Inspect(FileInfo src)
    {
        using var cmd = Cli
            .Wrap("ffprobe")
            .WithArguments([
                "-i", src.FullName,
                "-show_entries",
                "format=duration",
                "-v", "quiet",
                "-of", "csv=p=0"
            ])
            .ExecuteBufferedAsync();

        BufferedCommandResult? result = null;

        result = await cmd;

        var output = result.StandardOutput.Trim();

        if (string.IsNullOrWhiteSpace(output))
        {
            throw new InvalidDataException($"Expected to get duration from ffprobe for {src.FullName}");
        }

        if (float.TryParse(output, out float duration))
        {
            return new DurationResult(src.FullName, duration);
        }

        if ("N/A".Equals(output, StringComparison.OrdinalIgnoreCase))
        {
            // this came up at least once for one of the google memory videos, so lets just set to a small number and move on
            return new DurationResult(src.FullName, 15);
        }

        throw new InvalidOperationException($"Unable to parse duration [{output}] for {src.FullName}");
    }
}
