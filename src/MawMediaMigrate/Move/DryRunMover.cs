using MawMediaMigrate.Results;

namespace MawMediaMigrate.Move;

class DryRunMover
    : IMover
{
    public MoveResult Move(FileInfo src, FileInfo dst)
    {
        return new MoveResult
        {
            Src = src.FullName,
            Dst = dst.FullName
        };
    }
}
