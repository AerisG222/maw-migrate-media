using MawMediaMigrate.Results;

namespace MawMediaMigrate.VideoDuration;

class DurationProcessor
{
    // select distinct substring(path from '\.([^\.]*)$') as ext from media.file
    readonly string[] _vidExts = [
        ".3gp",
        ".avi",
        ".flv",
        ".m4v",
        ".mkv",
        ".mov",
        ".mp4",
        ".mpeg",
        ".mpg",
        ".VOB",
    ];
    readonly Lock _lockObj = new();
    readonly DirectoryInfo _mediaRoot;
    readonly DurationInspector _inspector = new();

    public DurationProcessor(Options options)
    {
        ArgumentNullException.ThrowIfNull(options);

        // this runs after moving, so use the dest dir - this also makes sure that the exif file path info reflects changes to dir names/etc.
        _mediaRoot = options.DestDir;
    }

    public async Task<IEnumerable<DurationResult>> InspectFiles()
    {
        var list = new List<DurationResult>();
        var files = _mediaRoot.EnumerateFiles("*", SearchOption.AllDirectories);

        await Parallel.ForEachAsync(files, async (file, token) =>
        {
            if (ShouldSkipFile(file))
            {
                return;
            }

            var result = await _inspector.Inspect(file);

            lock (_lockObj)
            {
                list.Add(result);
            }
        });

        return list;
    }

    bool ShouldSkipFile(FileInfo file) =>
        !file.Directory!.Name.Equals("src", StringComparison.OrdinalIgnoreCase) ||
        !_vidExts.Contains(file.Extension, StringComparer.OrdinalIgnoreCase);
}
