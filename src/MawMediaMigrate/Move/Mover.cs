using MawMediaMigrate.Results;

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

        var (width, height) = await _inspector.QueryDimensions(dst.FullName);

        return new MoveResult
        {
            Src = src.FullName,
            Dst = dst.FullName,
            Width = width,
            Height = height,
            Bytes = dst.Length
        };
    }
}
