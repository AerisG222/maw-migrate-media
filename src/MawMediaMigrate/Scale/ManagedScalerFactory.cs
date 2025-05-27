namespace MawMediaMigrate.Scale;

static class ManagedScalerFactory
{
    public static IManagedScaler Create(Options options)
    {
        return new ManagedScaler(
            options.OrigDir,
            new ImageScaler(options.OrigDir, options.DestDir),
            new VideoScaler(options.OrigDir, options.DestDir)
        );
    }
}
