using MawMediaMigrate.Results;

namespace MawMediaMigrate.Move;

class DryRunMover
    : IMover
{
    public Task<MoveResult> Move(FileInfo src, FileInfo dst)
    {
        return Task.FromResult(
            new MoveResult
            {
                Src = src.FullName,
                Dst = dst.FullName,
                Width = 100,
                Height = 200,
                Bytes = 300
            }
        );
    }
}
