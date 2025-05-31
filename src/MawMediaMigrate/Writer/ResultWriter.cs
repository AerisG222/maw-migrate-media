using System.Globalization;
using CsvHelper;
using MawMediaMigrate.Exif;
using MawMediaMigrate.Move;
using MawMediaMigrate.Scale;

namespace MawMediaMigrate.Writer;

class ResultWriter
    : IResultWriter
{
    readonly FileInfo _mappingFile;

    public ResultWriter(FileInfo mappingFile)
    {
        _mappingFile = mappingFile;
    }

    public async Task WriteMappingFile(IEnumerable<MoveResult> moveSpecs, IEnumerable<ExifResult> exifResults, IEnumerable<ScaleResult> scaledFiles)
    {
        using var writer = new StreamWriter(_mappingFile.FullName, false, System.Text.Encoding.UTF8);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        await csv.WriteRecordsAsync(moveSpecs);
    }
}
