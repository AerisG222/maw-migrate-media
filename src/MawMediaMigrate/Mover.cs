using System.Globalization;
using CsvHelper;

namespace MawMediaMigrate;

public class Mover
{
    readonly DirectoryInfo _origDir;
    readonly DirectoryInfo _destDir;
    readonly string _mappingFile;
    readonly Scaler _scaler = new();
    readonly ExifExporter _exifExporter = new ExifExporter();

    public Mover(DirectoryInfo origDir, DirectoryInfo destDir, string mappingFile)
    {
        _origDir = origDir;
        _destDir = destDir;
        _mappingFile = mappingFile;
    }

    public async Task MoveFiles()
    {
        var imageSpecs = MoveImages();
        var videoSpecs = MoveVideos();

        var moveSpecs = imageSpecs.Concat(videoSpecs);

        await WriteMappingFile(moveSpecs);
    }

    IEnumerable<MoveSpec> MoveImages()
    {
        return MoveOriginalMedia("images", ["src", "lg"]).ToBlockingEnumerable();
    }

    IEnumerable<MoveSpec> MoveVideos()
    {
        var renames = new Dictionary<string, string>
        {
            { "raw", "src" }
        };

        return MoveOriginalMedia("movies", ["raw"], renames).ToBlockingEnumerable();
    }

    async IAsyncEnumerable<MoveSpec> MoveOriginalMedia(
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
                await Scale(origFile, destFile);
                await ExportExif(origFile, destFile);

                // only log src files in the movespec list - all scaled media will be in diff dirs...
                if (destFile.Directory!.Name == "src")
                {
                    yield return new MoveSpec
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

    async Task Scale(FileInfo origFile, FileInfo dst)
    {
        if (origFile.Directory!.Name == "raw")
        {
            await _scaler.ScaleVideo(dst);
        }

        // this migration tool does not attempt to scale photos from source as that process is more complex and allows
        // for manipulations through things like rawtherapee pp3 files.  We only scale the lg so we retain overall look of image
        // by by scaling to avif, file size will be roughly 50%.
        if (origFile.Directory.Name == "lg")
        {
            await _scaler.ScaleImage(dst);
        }
    }

    async Task ExportExif(FileInfo origFile, FileInfo dst)
    {
        if (origFile.Directory!.Name == "raw" || origFile.Directory!.Name == "src")
        {
            await _exifExporter.ExportAsync(dst.FullName);
        }
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

    async Task WriteMappingFile(IEnumerable<MoveSpec> moveSpecs)
    {
        using var writer = new StreamWriter(_mappingFile);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        await csv.WriteRecordsAsync(moveSpecs);
    }
}
