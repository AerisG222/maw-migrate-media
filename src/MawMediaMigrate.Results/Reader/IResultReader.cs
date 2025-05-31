namespace MawMediaMigrate.Results.Reader;

public interface IResultReader
{
    Task<(IEnumerable<MoveResult> moveSpecs, IEnumerable<ExifResult> exifResults, IEnumerable<ScaleResult> scaledFiles)> ReadResults(DirectoryInfo srcDir);
}
