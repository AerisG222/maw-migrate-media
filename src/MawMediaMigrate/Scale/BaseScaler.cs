namespace MawMediaMigrate.Scale;

public abstract class BaseScaler
    : IScaler
{
    protected readonly Lock _lockObj = new();
    protected readonly Inspector _inspector = new();

    public abstract Task<ScaleResult> Scale(FileInfo src);

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
