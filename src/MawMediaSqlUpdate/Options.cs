namespace MawMediaSqlUpdate;

class Options
{
    public required DirectoryInfo SrcDir { get; init; }
    public required DirectoryInfo DestDir { get; init; }

    public static Options FromArgs(string[] args)
    {
        if (!(args.Length == 2))
        {
            ShowUsage();
            Environment.Exit(1);
        }

        var srcDir = new DirectoryInfo(args[0]);
        var destDir = new DirectoryInfo(args[1]);

        if (!srcDir.Exists)
        {
            Console.WriteLine("Invalid source directory specified.");
            Environment.Exit(2);
        }

        return new Options
        {
            SrcDir = srcDir,
            DestDir = destDir,
        };
    }

    static void ShowUsage()
    {
        Console.WriteLine("Usage: MawMediaSqlUpdate <src-dir> <dest-dir>");
        Console.WriteLine("  <src-dir> - Directory containing json result files from MawMediaMigrate.");
        Console.WriteLine("  <dest-dir> - Directory where sql update scripts should be written.");
    }
}
