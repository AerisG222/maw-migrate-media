using System.Collections.Concurrent;
using System.Text.Json;
using MawMediaMigrate.Results;

namespace MawMediaMigrate.Scale;

class ScaleProcessor
{
    const int SAVEPOINT_COUNT = 1000;
    int _savepointCount = 1;

    readonly Lock _lockObj = new();
    readonly DirectoryInfo _origDir;
    readonly DirectoryInfo _outDir;
    readonly IInspector _inspector;
    readonly IScaler _imageScaler;
    readonly IScaler _videoScaler;
    readonly HashSet<string> _seenDirs = [];

    public ScaleProcessor(Options options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _inspector = options.Inspector;
        _outDir = options.OutDir;

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
        var filesToSkip = ReadAllSavePoints()
            .Select(x => x.SrcPath)
            .ToDictionary(x => x);

        Console.WriteLine($"  - read {_savepointCount - 1} savepoints - these were already processed and will be skipped");

        await _inspector.BulkLoadSourceDimensions(_origDir.FullName);

        Console.WriteLine($"  - Completed bulk load dimensions");

        var imageResults = await ScaleImages(filesToSkip);
        var videoResults = await ScaleVideos(filesToSkip);

        return imageResults.Concat(videoResults);
    }

    Task<IEnumerable<ScaleResult>> ScaleImages(Dictionary<string, string> filesToSkip)
    {
        return ScaleMedia("images", "lg", _imageScaler, filesToSkip);
    }

    Task<IEnumerable<ScaleResult>> ScaleVideos(Dictionary<string, string> filesToSkip)
    {
        return ScaleMedia("movies", "raw", _videoScaler, filesToSkip);
    }

    async Task<IEnumerable<ScaleResult>> ScaleMedia(
        string mediaDirRootName,
        string mediaSrcScaleName,
        IScaler scaler,
        Dictionary<string, string> filesToSkip
    )
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

            if (filesToSkip.ContainsKey(file.FullName))
            {
                return;
            }

            ReportStatus(file);

            var scaleResult = await scaler.Scale(file, origMediaRoot);

            lock (_lockObj)
            {
                result.Add(scaleResult);

                if (result.Count() % SAVEPOINT_COUNT == 0)
                {
                    WriteSavePoint(result);
                    result.Clear();
                }
            }
        });

        WriteSavePoint(result);
        result.Clear();

        return ReadAllSavePoints();
    }

    void ReportStatus(FileInfo file)
    {
        var dirToReport = file.Directory!.Parent!.FullName;

        lock (_lockObj)
        {
            if (!_seenDirs.Contains(dirToReport))
            {
                _seenDirs.Add(dirToReport);

                Console.WriteLine($"  - {dirToReport}");
            }
        }
    }

    void WriteSavePoint(List<ScaleResult> results)
    {
        var filename = Path.Combine(_outDir.FullName, $"scale-savepoint-{_savepointCount}.json");

        using var fs = new FileStream(filename, FileMode.CreateNew, FileAccess.Write);

        JsonSerializer.Serialize(fs, results);

        _savepointCount++;
    }

    List<ScaleResult> ReadAllSavePoints()
    {
        var results = new List<ScaleResult>();
        var maxId = 0;

        foreach (var file in _outDir.EnumerateFiles("scale-savepoint-*.json"))
        {
            var id = int.Parse(
                Path.GetFileNameWithoutExtension(file.Name)
                .Replace("scale-savepoint-", string.Empty)
            );

            maxId = Math.Max(maxId, id);

            using var fs = new FileStream(file.FullName, FileMode.Open, FileAccess.Read);

            results.AddRange(JsonSerializer.Deserialize<ScaleResult[]>(fs)!);
        }

        _savepointCount = maxId + 1;

        return results;
    }
}
