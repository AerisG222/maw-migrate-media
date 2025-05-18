namespace MawMediaMigrate.Scale;

public class Scaler
{
    readonly Lock _lockObj = new();
    readonly DirectoryInfo _origDir;
    readonly ImageScaler _imageScaler;
    readonly VideoScaler _videoScaler;

    public Scaler(DirectoryInfo origDir, DirectoryInfo destDir)
    {
        _origDir = origDir;

        _imageScaler = new ImageScaler(origDir, destDir);
        _videoScaler = new VideoScaler(origDir, destDir);
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

            var scaleResult = await scaler.Scale(file);

            lock (_lockObj)
            {
                result.Add(scaleResult);
            }
        });

        return result;
    }
}
