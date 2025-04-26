using MawExifExport;

if (args.Length != 1)
{
    ShowUsage();
    Environment.Exit(1);
}

var rootDir = args[0];

if (string.IsNullOrEmpty(rootDir) || !Directory.Exists(rootDir))
{
    Console.WriteLine("Invalid directory specified.");
    Environment.Exit(1);
}

var exporter = new Exporter();
List<string> IGNORE_EXTENSIONS = [".pp3", ".json"];
var dirs = Directory.EnumerateDirectories(rootDir, "src", SearchOption.AllDirectories)
    .Concat(Directory.EnumerateDirectories(rootDir, "raw", SearchOption.AllDirectories));

foreach (var dir in dirs)
{
    await Parallel.ForEachAsync(Directory.EnumerateFiles(dir), async (file, cancellationToken) =>
    {
        if (IsMediaFile(file))
        {
            try
            {
                await exporter.ExportAsync(file);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing file {file}: {ex.Message}");
            }
        }
    });
}

bool IsMediaFile(string filePath)
{
    return !IGNORE_EXTENSIONS.Contains(Path.GetExtension(filePath), StringComparer.OrdinalIgnoreCase);
}

static void ShowUsage()
{
    Console.WriteLine("Usage: MawExifExport <root-dir>");
    Console.WriteLine("  <root-dir> - Directory to start looking for photos / videos in the legacy directory structure.");
}