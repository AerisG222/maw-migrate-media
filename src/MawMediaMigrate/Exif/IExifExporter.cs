using MawMediaMigrate.Results;

namespace MawMediaMigrate.Exif;

interface IExifExporter
{
    Task<ExifResult> Export(FileInfo file, FileInfo outfile);
}
