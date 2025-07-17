using MawAuditAssets.Database;

if (args.Length != 2)
{
    ShowUsage();
    Environment.Exit(1);
}

var dbConnString = args[0];
var assetRootDir = new DirectoryInfo(args[1]);

if (!assetRootDir.Exists)
{
    Console.WriteLine("Asset Root Dir not found!");
    Environment.Exit(2);
}

var dbReader = new DbReader(dbConnString);
var filesFromDb = await dbReader.LoadExpectedFilesFromDatabase();
var filesFromFilesystem = assetRootDir.EnumerateFiles("*", SearchOption.AllDirectories);

Console.WriteLine($"db files: {filesFromDb.Count()}");
Console.WriteLine($"fs files: {filesFromFilesystem.Count()}");

static void ShowUsage()
{
    Console.WriteLine("Usage: MawDbMigrate <db-conn-string> <sql-file>");
    Console.WriteLine("  <db-conn-string> - Connection string to the database.");
    Console.WriteLine("  <outdir> - Directory where SQL scripts should be written.");
}
