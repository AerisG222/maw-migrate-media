using MawMediaMigrate.Results;

class MediaRepo
{
    readonly Dictionary<string, MediaInfo> _dict = [];
    readonly DirectoryInfo _origMediaDir;
    readonly DirectoryInfo _destMediaDir;

    public MediaRepo(DirectoryInfo origMediaDir, DirectoryInfo destMediaDir)
    {
        ArgumentNullException.ThrowIfNull(origMediaDir);
        ArgumentNullException.ThrowIfNull(destMediaDir);

        _origMediaDir = origMediaDir;
        _destMediaDir = destMediaDir;
    }

    public void AddMoveInfo(IEnumerable<MoveResult> results)
    {
        foreach (var result in results)
        {
            var mi = GetMediaInfo(result.Dst);

            mi.OriginalSrcPath = result.Src;
        }
    }

    public void AddExifInfo(IEnumerable<ExifResult> results)
    {
        foreach (var result in results)
        {
            var mi = GetMediaInfo(result.Src);

            mi.ExifFile = result.ExifFile;
        }
    }

    public void AddScaleInfo(IEnumerable<ScaleResult> results)
    {
        foreach (var result in results)
        {
            // we will need to treat this special as we scaled the 'lg' images as those had all the adjustments applied at full resolution -
            // rather than using the src images, many of which are raw and when through various processing over the years.  so here we need
            // to more flexibly match the scaled 'lg' files to the 'src' files.
        }
    }

    MediaInfo GetMediaInfo(string path)
    {
        var dst = CleanDestinationPath(path);

        if (_dict.TryGetValue(path, out var mediaInfo))
        {
            return mediaInfo;
        }

        mediaInfo = new MediaInfo
        {
            DestinationSrcPath = dst
        };

        _dict[dst] = mediaInfo;

        return mediaInfo;
    }

    string CleanDestinationPath(string path)
    {
        return path
            .Replace(_origMediaDir.FullName, string.Empty, StringComparison.OrdinalIgnoreCase);
    }
}
