using System.Collections.Immutable;
using MawAuditAssets.Database;

if (args.Length != 2)
{
    ShowUsage();
    Environment.Exit(1);
}

var dbConnString = args[0];
var assetRootDir = args[1];

if (!Directory.Exists(assetRootDir))
{
    Console.WriteLine("Asset Root Dir not found!");
    Environment.Exit(2);
}

var dbReader = new DbReader(dbConnString);

var filesFromDb =
    (await dbReader.LoadExpectedFilesFromDatabase())
    .Select(x => Path.Combine(assetRootDir, x[1..]))
    .ToImmutableSortedSet(StringComparer.Ordinal);

var filesFromFilesystem = Directory
    .EnumerateFiles(assetRootDir, "*", SearchOption.AllDirectories)
    .Where(f => !string.Equals(".dng", Path.GetExtension(f), StringComparison.OrdinalIgnoreCase))
    .ToImmutableSortedSet(StringComparer.Ordinal);

Console.WriteLine($"db files: {filesFromDb.Count}");
Console.WriteLine($"fs files: {filesFromFilesystem.Count}");

var dbOnly = filesFromDb.Except(filesFromFilesystem);
var fsOnly = filesFromFilesystem.Except(filesFromDb);

Console.WriteLine("** DB ONLY **");
WriteOutput(dbOnly, "DB");
Console.WriteLine("");
Console.WriteLine("");
Console.WriteLine("** FS ONLY **");
WriteOutput(fsOnly, "FS");
Console.WriteLine("");

static void WriteOutput(IEnumerable<string> files, string prefix)
{
    foreach (var f in files)
    {
        Console.WriteLine($"  - {prefix}: {f}");
    }
}

static void ShowUsage()
{
    Console.WriteLine("Usage: MawDbMigrate <db-conn-string> <sql-file>");
    Console.WriteLine("  <db-conn-string> - Connection string to the database.");
    Console.WriteLine("  <outdir> - Directory where SQL scripts should be written.");
}
