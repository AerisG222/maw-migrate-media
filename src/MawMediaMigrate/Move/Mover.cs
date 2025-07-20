using MawMediaMigrate.Results;
using MawMediaMigrate.Scale;

namespace MawMediaMigrate.Move;

class Mover
    : IMover
{
    readonly IInspector _inspector;

    public Mover(IInspector inspector)
    {
        ArgumentNullException.ThrowIfNull(inspector);

        _inspector = inspector;
    }

    public async Task<MoveResult> Move(FileInfo src, FileInfo dst)
    {
        if (!dst.Directory!.Exists)
        {
            dst.Directory.Create();
        }

        File.Move(src.FullName, dst.FullName);

        var dims = await _inspector.QueryDimensions(dst.FullName);

        return new MoveResult
        {
            Src = src.FullName,
            Dst = dst.FullName,
            Width = dims.ImageWidth,
            Height = dims.ImageHeight,
            Bytes = dst.Length
        };
    }
}
