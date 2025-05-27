using MawMediaMigrate.Move;

namespace MawMediaMigrate.Writer;

interface IResultWriter
{
    Task WriteMappingFile(string outfile, IEnumerable<MoveResult> moveSpecs);
}
