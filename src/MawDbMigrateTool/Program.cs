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
string sqlFile = args[1];

if (string.IsNullOrWhiteSpace(dbConnString))
{
    Console.WriteLine("The database connection string cannot be null or empty.");
    Environment.Exit(2);
}
if (string.IsNullOrWhiteSpace(sqlFile))
{
    Console.WriteLine("The SQL file path cannot be null or empty.");
    Environment.Exit(3);
}
if (File.Exists(sqlFile))
{
    Console.WriteLine($"The specified SQL file exists: {sqlFile}");
    Environment.Exit(4);
}

var exporter = new Exporter(dbConnString, sqlFile);

await exporter.ExportDatabase();

void ShowUsage()
{
    Console.WriteLine("Usage: MawDbMigrateTool <db-conn-string> <sql-file>");
    Console.WriteLine("  <db-conn-string> - Connection string to the database.");
    Console.WriteLine("  <sql-file> - Path to the SQL file to write, which can then be directly used by psql for import.");
}