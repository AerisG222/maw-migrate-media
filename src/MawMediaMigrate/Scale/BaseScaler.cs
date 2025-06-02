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

    protected static IEnumerable<ScaleSpec> GetScalesForDimensions(int width, int height, bool includePosters)
    {
        foreach (var scale in ScaleSpec.AllScales)
        {
            if (scale.IsPoster && !includePosters)
            {
                continue;
            }

            // if either dimension is greater than the scale bounds, scale it
            if (width > scale.Width || height > scale.Height)
            {
                yield return scale;
            }
            else
            {
                // if we are here, the item fits in the scale bounds, return this last scale so that we can keep the
                // highest res that fits
                yield return scale;
                yield break;
            }
        }
    }

    protected static void CreateDir(string dir)
    {
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }
}
