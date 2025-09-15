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
    .Where(f => !string.Equals(".sql", Path.GetExtension(f), StringComparison.OrdinalIgnoreCase))
    .Where(f => !string.Equals(".pp3", Path.GetExtension(f), StringComparison.OrdinalIgnoreCase))
    .Where(f => !f.EndsWith("pp3s.tar.gz"))
    .Where(f => f.IndexOf("/2k/") < 0)
    .Where(f => f.IndexOf("/4k/") < 0)
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
    Console.WriteLine("Usage: MawAuditAssets <legacy-db-conn-string> <legacy-asset-root-dir>");
    Console.WriteLine("  <legacy-db-conn-string> - Connection string to the database.");
    Console.WriteLine("  <legacy-asset-root-dir> - Directory where legacy asset files were written.");
}
