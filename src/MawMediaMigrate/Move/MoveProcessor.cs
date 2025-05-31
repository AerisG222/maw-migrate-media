using MawMediaMigrate.Results;

namespace MawMediaMigrate.Move;

class MoveProcessor
{
    readonly Lock _lockObj = new();
    readonly IMover _mover;
    readonly DirectoryInfo _origDir;
    readonly DirectoryInfo _destDir;

    public MoveProcessor(Options options)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(options.OrigDir);
        ArgumentNullException.ThrowIfNull(options.DestDir);

        _mover = options.DryRun ? new DryRunMover() : new Mover();
        _origDir = options.OrigDir;
        _destDir = options.DestDir;
    }

    public IEnumerable<MoveResult> MoveFiles()
    {
        var imageResults = MoveOriginalMedia("images", "src");
        var videoResults = MoveOriginalMedia("movies", "raw");

        return imageResults.Concat(videoResults);
    }

    IEnumerable<MoveResult> MoveOriginalMedia(
        string origMediaDirName,
        string dirToMove
    )
    {
        var list = new List<MoveResult>();
        var srcDir = new DirectoryInfo(Path.Combine(_origDir.FullName, origMediaDirName));

        if (!srcDir.Exists)
        {
            throw new DirectoryNotFoundException($"The directory {srcDir.FullName} does not exist.");
        }

        var files = srcDir.EnumerateFiles("*", SearchOption.AllDirectories);

        Parallel.ForEach(files, (file, token) =>
        {
            if (string.Equals(dirToMove, file.Directory!.Name, StringComparison.OrdinalIgnoreCase))
            {
                var destFile = BuildFileDest(srcDir, file);

                var result = _mover.Move(file, destFile);

                lock (_lockObj)
                {
                    list.Add(result);
                }
            }
        });

        return list;
    }

    FileInfo BuildFileDest(DirectoryInfo origRootDir, FileInfo file)
    {
        // 1. directory.name will either be src or raw - so go up one dir and force the dir for the file to be src
        // 2. prefer dashes to underscores (we shouldn't have any spaces, but lets be safe) for directory names
        var destDirName = Path.Combine(file.Directory!.Parent!.FullName, "src")
            .Replace(origRootDir.FullName, _destDir.FullName)
            .FixupMediaDirectory();

        return new FileInfo(Path.Combine(destDirName, file.Name));
    }
}
