namespace MawMediaSqlUpdate;

class Options
{
    public required DirectoryInfo OrigMediaDir { get; init; }
    public required DirectoryInfo DestMediaDir { get; init; }
    public required DirectoryInfo JsonResultsDir { get; init; }
    public required DirectoryInfo SqlOutputDir { get; init; }

    public static Options FromArgs(string[] args)
    {
        if (!(args.Length == 4))
        {
            ShowUsage();
            Environment.Exit(1);
        }

        var origMediaDir = new DirectoryInfo(args[0]);
        var destMediaDir = new DirectoryInfo(args[1]);
        var jsonResultsDir = new DirectoryInfo(args[2]);
        var sqlOutputDir = new DirectoryInfo(args[3]);

        if (!jsonResultsDir.Exists)
        {
            Console.WriteLine("Invalid source directory specified.");
            Environment.Exit(2);
        }

        // if (!destMediaDir.Exists)
        // {
        //     Console.WriteLine("Invalid destination media directory specified.");
        //     Environment.Exit(3);
        // }

        return new Options
        {
            OrigMediaDir = origMediaDir,
            DestMediaDir = destMediaDir,
            JsonResultsDir = jsonResultsDir,
            SqlOutputDir = sqlOutputDir
        };
    }

    static void ShowUsage()
    {
        Console.WriteLine("Usage: MawMediaSqlUpdate <orig-media-dir> <dest-media-dir> <json-results-dir> <sql-output-dir>");
        Console.WriteLine("  <orig-asset-dir>   - Root Directory where photos / videos in the legacy directory structure. (i.e. /data/www/website_assets).");
        Console.WriteLine("  <dest-asset-dir>   - Root Directory for the new media (i.e. /data/maw-media-assets).");
        Console.WriteLine("  <json-results-dir> - Directory containing json result files from MawMediaMigrate.");
        Console.WriteLine("  <sql-output-dir>   - Directory where sql update scripts should be written.");
    }
}
