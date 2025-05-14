using MawMediaMigrate;

if (args.Length != 3)
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

var mover = new Mover(origDir, destDir);
var writer = new ResultWriter();

var results = mover.MoveFiles();

await writer.WriteMappingFile(mappingFile, results);

static void ShowUsage()
{
    Console.WriteLine("Usage: MawMediaMigrate <orig-websiteassets-dir> <destination-media-dir> <mapping-file>");
    Console.WriteLine("  <orig-websiteassets-dir> - Directory to start looking for photos / videos in the legacy directory structure.");
    Console.WriteLine("  <destination-media-dir> - Directory to move the files to.");
    Console.WriteLine("  <mapping-file> - File to store the mapping of original file paths to new file paths.");
}
