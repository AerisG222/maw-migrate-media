using MawMediaMigrate.Results;

namespace MawMediaMigrate.Move;

interface IMover
{
    MoveResult Move(FileInfo file, FileInfo destFile);
}
