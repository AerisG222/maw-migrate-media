using MawMediaMigrate.Exif;
using MawMediaMigrate.Move;
using MawMediaMigrate.Scale;

namespace MawMediaMigrate.Writer;

interface IResultWriter
{
    Task WriteMappingFile(IEnumerable<MoveResult> moveSpecs, IEnumerable<ExifResult> exifResults, IEnumerable<ScaleResult> scaledFiles);
}
