using Dapper;
using MawDbMigrateTool;

[module:DapperAot]
DefaultTypeMap.MatchNamesWithUnderscores = true;

if (args.Length != 2)
{
    ShowUsage();
    Environment.Exit(1);
}

string dbConnString = args[0];
string outdir = args[1];

if (string.IsNullOrWhiteSpace(dbConnString))
{
    Console.WriteLine("The database connection string cannot be null or empty.");
    Environment.Exit(2);
}
if (string.IsNullOrWhiteSpace(outdir))
{
    Console.WriteLine("The output path cannot be null or empty.");
    Environment.Exit(3);
}
if (Directory.Exists(outdir))
{
    Console.WriteLine($"The specified output directory exists: {outdir}");
    Environment.Exit(4);
}

var exporter = new Exporter(dbConnString, outdir);

await exporter.ExportDatabase();

static void ShowUsage()
{
    Console.WriteLine("Usage: MawDbMigrateTool <db-conn-string> <sql-file>");
    Console.WriteLine("  <db-conn-string> - Connection string to the database.");
    Console.WriteLine("  <outdir> - Directory where SQL scripts should be written.");
}