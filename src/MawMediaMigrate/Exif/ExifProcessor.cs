using MawMediaMigrate.Results;

namespace MawMediaMigrate.Exif;

class ExifProcessor
{
    readonly Lock _lockObj = new();
    readonly bool _isDryRun;
    readonly IExifExporter _exporter;
    readonly DirectoryInfo _mediaRoot;

    public ExifProcessor(Options options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _isDryRun = options.DryRun;
        _exporter = options.DryRun ? new DryRunExifExporter() : new ExifExporter();

        // this runs after moving, so use the dest dir - this also makes sure that the exif file path info reflects changes to dir names/etc.
        _mediaRoot = options.DestDir;
    }

    public async Task<IEnumerable<ExifResult>> ExportExifData()
    {
        if (_isDryRun)
        {
            return [];
        }

        var list = new List<ExifResult>();
        var files = _mediaRoot.EnumerateFiles("*", SearchOption.AllDirectories);

        await Parallel.ForEachAsync(files, async (file, token) =>
        {
            if (file.Directory!.Name != "src")
            {
                return;
            }

            var outfile = new FileInfo($"{file.FullName}.json");
            var result = await _exporter.Export(file, outfile);

            lock (_lockObj)
            {
                list.Add(result);
            }
        });

        return list;
    }
}
