using MawMediaMigrate.Results;

namespace MawMediaMigrate.Move;

class Mover
    : IMover
{
    public MoveResult Move(FileInfo src, FileInfo dst)
    {
        if (!dst.Directory!.Exists)
        {
            dst.Directory.Create();
        }

        File.Move(src.FullName, dst.FullName);

        return new MoveResult
        {
            Src = src.FullName,
            Dst = dst.FullName
        };
    }
}
