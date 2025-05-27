namespace MawMediaMigrate.Exif;

interface IExifExporter
{
    Task<IEnumerable<ExifResult>> ExportExifData(DirectoryInfo mediaRoot);
}
