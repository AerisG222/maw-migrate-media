namespace MawMediaMigrate;

class Options
{
    public required DirectoryInfo OrigDir { get; init; }
    public required DirectoryInfo DestDir { get; init; }
    public required FileInfo MappingFile { get; init; }
    public bool DryRun { get; init; }

    public static Options FromArgs(string[] args)
    {
        if (!(args.Length == 3 || args.Length == 4))
        {
            ShowUsage();
            Environment.Exit(1);
        }

        var origDir = new DirectoryInfo(args[0]);
        var destDir = new DirectoryInfo(args[1]);
        var mappingFile = args[2];

        if (!origDir.Exists || !string.Equals("website_assets", origDir.Name, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Invalid original directory specified.");
            Environment.Exit(2);
        }

        if (destDir.Exists || !destDir.Name.Equals("media", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Destination exists or is not named 'media' as expected.");
            Environment.Exit(3);
        }

        if (string.IsNullOrEmpty(mappingFile) || File.Exists(mappingFile))
        {
            Console.WriteLine("Invalid mapping file specified or already exists.");
            Environment.Exit(4);
        }

        var dryRun = false;

        if (args.Length == 4)
        {
            dryRun = "--dry-run".Equals(args[3], StringComparison.OrdinalIgnoreCase);
        }

        return new Options
        {
            OrigDir = origDir,
            DestDir = destDir,
            MappingFile = new FileInfo(mappingFile),
            DryRun = dryRun
        };
    }

    static void ShowUsage()
    {
        Console.WriteLine("Usage: MawMediaMigrate <orig-websiteassets-dir> <destination-media-dir> <mapping-file> [--dry-run]");
        Console.WriteLine("  <orig-websiteassets-dir> - Directory to start looking for photos / videos in the legacy directory structure.");
        Console.WriteLine("  <destination-media-dir> - Directory to move the files to.");
        Console.WriteLine("  <mapping-file> - File to store the mapping of original file paths to new file paths.");
        Console.WriteLine("  --dry-run - Do not perform any operations, just show what would be done.");
    }
}
