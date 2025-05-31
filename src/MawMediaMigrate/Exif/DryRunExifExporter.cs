namespace MawMediaMigrate.Exif;

class DryRunExifExporter
    : IExifExporter
{
    public Task<ExifResult> Export(FileInfo file, FileInfo outfile)
    {
        return Task.FromResult(new ExifResult
        {
            Src = file.FullName,
            ExifFile = outfile.FullName
        });
    }
}
