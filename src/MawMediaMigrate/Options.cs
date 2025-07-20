using MawMediaMigrate.Scale;

namespace MawMediaMigrate;

class Options
{
    public required DirectoryInfo OrigDir { get; init; }
    public required DirectoryInfo DestDir { get; init; }
    public required DirectoryInfo OutDir { get; init; }
    public bool DryRun { get; init; }
    public required IInspector Inspector { get; init; }

    public static Options FromArgs(string[] args)
    {
        if (!(args.Length == 3 || args.Length == 4))
        {
            ShowUsage();
            Environment.Exit(1);
        }

        var origDir = new DirectoryInfo(args[0]);
        var destDir = new DirectoryInfo(args[1]);
        var outDir = new DirectoryInfo(args[2]);

        if (!origDir.Exists || !string.Equals("website_assets", origDir.Name, StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Invalid original directory specified.");
            Environment.Exit(2);
        }

        if (!destDir.Name.Equals("maw-media-assets", StringComparison.OrdinalIgnoreCase))
        {
            Console.WriteLine("Destination must be named 'maw-media-assets'");
            Environment.Exit(3);
        }

        if (destDir.Exists)
        {
            Console.WriteLine("** Destination exists - process will scan for unprocessed files and resume where it left off.");
        }

        if (!outDir.Exists)
        {
            outDir.Create();
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
            OutDir = outDir,
            DryRun = dryRun,
            Inspector = dryRun
                ? new DryRunInspector()
                : new Inspector()
        };
    }

    Options()
    {
        // hide
    }

    static void ShowUsage()
    {
        Console.WriteLine("Usage: MawMediaMigrate <orig-websiteassets-dir> <destination-media-dir> <out-dir> [--dry-run]");
        Console.WriteLine("  <orig-websiteassets-dir> - Directory to start looking for photos / videos in the legacy directory structure.");
        Console.WriteLine("  <destination-media-dir> - Directory to move the files to.");
        Console.WriteLine("  <out-dir> - Directory where results should be written to - mover / scaler / exif, so they can later be used to build sql scripts.");
        Console.WriteLine("  --dry-run - Do not perform any operations, just show what would be done.");
    }
}
