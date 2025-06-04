using MawMediaMigrate.Results;

namespace MawMediaMigrate.Move;

interface IMover
{
    Task<MoveResult> Move(FileInfo file, FileInfo destFile);
}
