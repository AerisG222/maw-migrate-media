namespace MawMediaMigrate.Results.Writer;

public interface IResultWriter
{
    Task WriteResults(IEnumerable<MoveResult> moveSpecs, IEnumerable<ExifResult> exifResults, IEnumerable<ScaleResult> scaledFiles);
}
