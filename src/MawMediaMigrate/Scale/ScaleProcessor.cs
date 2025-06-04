using MawMediaMigrate.Results;

namespace MawMediaMigrate.Scale;

class ScaleProcessor
{
    readonly Lock _lockObj = new();
    readonly DirectoryInfo _origDir;
    readonly IScaler _imageScaler;
    readonly IScaler _videoScaler;

    public ScaleProcessor(Options options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _imageScaler = options.DryRun
            ? new DryRunPhotoScaler(options.Inspector, options.OrigDir, options.DestDir)
            : new PhotoScaler(options.Inspector, options.OrigDir, options.DestDir);

        _videoScaler = options.DryRun
            ? new DryRunVideoScaler(options.Inspector, options.OrigDir, options.DestDir)
            : new VideoScaler(options.Inspector, options.OrigDir, options.DestDir);

        _origDir = options.OrigDir;
    }

    public async Task<IEnumerable<ScaleResult>> ScaleFiles()
    {
        var imageResults = await ScaleImages();
        var videoResults = await ScaleVideos();

        return imageResults.Concat(videoResults);
    }

    Task<IEnumerable<ScaleResult>> ScaleImages()
    {
        return ScaleMedia("images", "lg", _imageScaler);
    }

    Task<IEnumerable<ScaleResult>> ScaleVideos()
    {
        return ScaleMedia("movies", "raw", _videoScaler);
    }

    async Task<IEnumerable<ScaleResult>> ScaleMedia(string mediaDirRootName, string mediaSrcScaleName, IScaler scaler)
    {
        var result = new List<ScaleResult>();
        var origMediaRoot = new DirectoryInfo(Path.Combine(_origDir.FullName, mediaDirRootName));
        var files = origMediaRoot.EnumerateFiles("*", SearchOption.AllDirectories);

        await Parallel.ForEachAsync(files, async (file, token) =>
        {
            if (file.Directory!.Name != mediaSrcScaleName)
            {
                return;
            }

            var scaleResult = await scaler.Scale(file, origMediaRoot);

            lock (_lockObj)
            {
                result.Add(scaleResult);
            }
        });

        return result;
    }
}
