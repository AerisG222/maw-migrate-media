namespace MawMediaMigrate.Exif;

static class ExifExporterFactory
{
    public static IExifExporter Create(Options options)
    {
        return new ExifExporter();
    }
}
