using System.Globalization;
using CsvHelper;
using MawMediaMigrate.Move;

namespace MawMediaMigrate.Writer;

class ResultWriter : IResultWriter
{
    public async Task WriteMappingFile(string outfile, IEnumerable<MoveResult> moveSpecs)
    {
        using var writer = new StreamWriter(outfile);
        using var csv = new CsvWriter(writer, CultureInfo.InvariantCulture);

        await csv.WriteRecordsAsync(moveSpecs);
    }
}
