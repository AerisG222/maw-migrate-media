namespace MawMediaMigrate;

public abstract class BaseScaler
    : IScaler
{
    protected readonly Lock _lockObj = new();
    protected readonly Inspector _inspector = new();

    public abstract Task<IEnumerable<ScaledFile>> Scale(FileInfo src);

    protected static bool ShouldScale(int width, int height, Scale scale)
    {
        return width >= scale.Width || height >= scale.Height;
    }

    protected static void SafeCreateDir(string dir)
    {
        if (!Directory.Exists(dir))
        {
            try
            {
                Directory.CreateDirectory(dir);
            }
            catch
            {
                // swallow
            }
        }
    }
}
