using MawMediaMigrate.Results;

class MediaRepo
{
    // we scaled the 'lg' images as those had all the adjustments applied at full resolution rather than from src
    // so for those, we need to additionally provide a mapping from lg to new src files.
    readonly Dictionary<string, string> _oldSrcToNewSrcDict = [];
    readonly Dictionary<string, MediaInfo> _dict = [];
    readonly DirectoryInfo _origMediaDir;

    public MediaRepo(DirectoryInfo origMediaDir)
    {
        ArgumentNullException.ThrowIfNull(origMediaDir);

        _origMediaDir = origMediaDir;
    }

    public void AssembleMediaInfo(
        IEnumerable<MoveResult> moveResults,
        IEnumerable<ExifResult> exifResults,
        IEnumerable<ScaleResult> scaledFiles
    )
    {
        // order is important, move needs to come first as this provides the base mapping
        AddMoveInfo(moveResults);
        AddExifInfo(exifResults);
        AddScaleInfo(scaledFiles);
    }

    public IEnumerable<MediaInfo> GetMediaInfos() => _dict.Values;

    void AddMoveInfo(IEnumerable<MoveResult> results)
    {
        foreach (var result in results)
        {
            CreateMediaInfo(result);
        }
    }

    void AddExifInfo(IEnumerable<ExifResult> results)
    {
        foreach (var result in results)
        {
            var mi = GetMediaInfo(result.Src);

            mi.ExifFile = result.ExifFile;
        }
    }

    void AddScaleInfo(IEnumerable<ScaleResult> results)
    {
        foreach (var result in results)
        {
            // video src/raw files should match up, so try to to look that up first
            if (!_dict.TryGetValue(result.SrcPath, out var mi))
            {
                try
                {
                    mi = GetMediaInfoByAlternateKey(result.SrcPath);
                }
                catch (Exception)
                {
                    Console.WriteLine($"** did not find: {result.SrcPath}");
                    continue;
                }
            }

            mi.ScaledFiles = result.ScaledFiles;
        }
    }

    MediaInfo GetMediaInfo(string path) => _dict[CleanDestinationPath(path)];
    MediaInfo GetMediaInfoByAlternateKey(string path) => _dict[_oldSrcToNewSrcDict[CleanAltKeyPath(path)]];

    void CreateMediaInfo(MoveResult result)
    {
        var dst = CleanDestinationPath(result.Dst);

        var mediaInfo = new MediaInfo
        {
            DestinationSrcPath = dst,
            OriginalSrcPath = result.Src
        };

        _dict[dst] = mediaInfo;
        _oldSrcToNewSrcDict[CleanAltKeyPath(result.Src)] = dst;
    }

    string CleanDestinationPath(string path)
    {
        return path
            .Replace(_origMediaDir.FullName, string.Empty, StringComparison.OrdinalIgnoreCase);
    }

    string CleanAltKeyPath(string path)
    {
        var file = new FileInfo(
            path
                .Replace(_origMediaDir.FullName, string.Empty, StringComparison.OrdinalIgnoreCase)
                .Replace("/lg/", "/")
                .Replace("/src/", "/")
        );

        return Path.Combine(file.Directory?.FullName ?? string.Empty, Path.GetFileNameWithoutExtension(file.Name));
    }
}
