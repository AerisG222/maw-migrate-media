namespace MawMediaMigrate;

public class Mover
{
    readonly DirectoryInfo _origDir;
    readonly DirectoryInfo _destDir;
    readonly ImageScaler _imageScaler = new();
    readonly VideoScaler _videoScaler = new();
    readonly ExifExporter _exifExporter = new();

    public Mover(DirectoryInfo origDir, DirectoryInfo destDir)
    {
        _origDir = origDir;
        _destDir = destDir;
    }

    public IEnumerable<MoveResult> MoveFiles()
    {
        var imageResults = MoveImages();
        var videoResults = MoveVideos();

        return imageResults.Concat(videoResults);
    }

    IEnumerable<MoveResult> MoveImages()
    {
        return MoveOriginalMedia("images", ["src", "lg"]).ToBlockingEnumerable();
    }

    IEnumerable<MoveResult> MoveVideos()
    {
        var renames = new Dictionary<string, string>
        {
            { "raw", "src" }
        };

        return MoveOriginalMedia("movies", ["raw"], renames).ToBlockingEnumerable();
    }

    async IAsyncEnumerable<MoveResult> MoveOriginalMedia(
        string origMediaDirName,
        string[] dirsToKeep,
        Dictionary<string, string>? renameMap = null
    ) {
        var dir = new DirectoryInfo(Path.Combine(_origDir.FullName, origMediaDirName));

        if(!dir.Exists)
        {
            throw new DirectoryNotFoundException($"The directory {dir.FullName} does not exist.");
        }

        var files = dir.EnumerateFiles("*", SearchOption.AllDirectories);

        foreach (var file in files)
        {
            if (dirsToKeep.Contains(file.Directory!.Name))
            {
                // get another ref to this file that won't change due to the move
                var origFile = new FileInfo(file.FullName);
                var destFile = BuildFileDest(dir, file, renameMap);

                Move(file, destFile);
                var scaledFiles = await Scale(origFile, destFile);
                var exifFile = await ExportExif(destFile);

                // only log src files in the movespec list - all scaled media will be in diff dirs...
                if (destFile.Directory!.Name == "src")
                {
                    yield return new MoveResult
                    {
                        Src = origFile.FullName,
                        Dst = destFile.FullName
                    };
                }
            }
        }
    }

    static void Move(FileInfo src, FileInfo dst)
    {
        if (!dst.Directory!.Exists)
        {
            dst.Directory.Create();
        }

        src.MoveTo(dst.FullName);
    }

    async Task<IEnumerable<ScaledFile>> Scale(FileInfo origFile, FileInfo dst)
    {
        if (origFile.Directory!.Name == "raw")
        {
            return await _videoScaler.Scale(dst);
        }

        // this migration tool does not attempt to scale photos from source as that process is more complex and allows
        // for manipulations through things like rawtherapee pp3 files.  We only scale the lg so we retain overall look of image
        // by by scaling to avif, file size will be roughly 50%.
        if (origFile.Directory.Name == "lg")
        {
            return await _imageScaler.Scale(dst);
        }

        return [];
    }

    async Task<string?> ExportExif(FileInfo dst)
    {
        if (dst.Directory!.Name == "src")
        {
            return await _exifExporter.ExportAsync(dst.FullName);
        }

        return null;
    }

    FileInfo BuildFileDest(DirectoryInfo origRootDir, FileInfo file, Dictionary<string, string>? renameMap = null)
    {
        string destDirName = file.Directory!.FullName;

        if (renameMap != null && renameMap.TryGetValue(file.Directory.Name, out string? value))
        {
            destDirName = Path.Combine(file.Directory.Parent!.FullName, value);
        }

        // prefer dashes to underscores (we shouldn't have any spaces, but lets be safe) for directory names
        destDirName = destDirName
            .Replace(origRootDir.FullName, _destDir.FullName)
            .Replace(" ", "-")
            .Replace("_", "-");

        return new FileInfo(Path.Combine(destDirName, file.Name));
    }
}
