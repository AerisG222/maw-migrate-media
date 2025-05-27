namespace MawMediaMigrate.Writer;

static class ResultWriterFactory
{
    public static IResultWriter Create(Options options)
    {
        return new ResultWriter();
    }
}
