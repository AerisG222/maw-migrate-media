using MawMediaMigrate.Results;

namespace MawMediaMigrate.Scale;

abstract class BaseScaler
    : IScaler
{
    protected readonly Lock _lockObj = new();
    protected readonly IInspector _inspector;
    protected readonly DirectoryInfo _origRootDir;
    protected readonly DirectoryInfo _destRootDir;

    public BaseScaler(IInspector inspector, DirectoryInfo origRootDir, DirectoryInfo destRootDir)
    {
        ArgumentNullException.ThrowIfNull(inspector);
        ArgumentNullException.ThrowIfNull(origRootDir);
        ArgumentNullException.ThrowIfNull(destRootDir);

        _inspector = inspector;
        _origRootDir = origRootDir;
        _destRootDir = destRootDir;
    }

    public abstract Task<ScaleResult> Scale(FileInfo src, DirectoryInfo origMediaRoot);

    protected static bool ShouldScale(int width, int height, ScaleSpec scale)
    {
        return width >= scale.Width || height >= scale.Height;
    }

    protected static void CreateDir(string dir)
    {
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
}
