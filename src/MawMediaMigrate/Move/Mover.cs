namespace MawMediaMigrate.Move;

public class Mover
{
    readonly Lock _lockObj = new();
    readonly DirectoryInfo _origDir;
    readonly DirectoryInfo _destDir;

    public Mover(DirectoryInfo origDir, DirectoryInfo destDir)
    {
        _origDir = origDir;
        _destDir = destDir;
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
        var results = new List<MoveResult>();
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
                // get another ref to this file that won't change due to the move
                var origFile = file.FullName;
                var destFile = BuildFileDest(srcDir, file);

                Move(file, destFile);

                lock (_lockObj)
                {
                    results.Add(new MoveResult
                    {
                        Src = origFile,
                        Dst = destFile.FullName
                    });
                }
            }
        });

        return results;
    }

    static void Move(FileInfo src, FileInfo dst)
    {
        if (!dst.Directory!.Exists)
        {
            dst.Directory.Create();
        }

        src.MoveTo(dst.FullName);
    }

    FileInfo BuildFileDest(DirectoryInfo origRootDir, FileInfo file)
    {
        // 1. directory.name will either be src or raw - so go up one dir and force the dir for the file to be src
        // 2. prefer dashes to underscores (we shouldn't have any spaces, but lets be safe) for directory names
        var destDirName = Path.Combine(file.Directory!.Parent!.FullName, "src")
            .Replace(origRootDir.FullName, _destDir.FullName)
            .Replace(" ", "-")
            .Replace("_", "-");

        return new FileInfo(Path.Combine(destDirName, file.Name));
    }
}
