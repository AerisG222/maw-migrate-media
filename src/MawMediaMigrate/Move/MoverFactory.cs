namespace MawMediaMigrate.Move;

static class MoverFactory
{
    public static IMover Create(Options options)
    {
        return new Mover(options.OrigDir, options.DestDir);
    }
}
