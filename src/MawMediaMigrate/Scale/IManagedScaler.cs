namespace MawMediaMigrate.Scale;

interface IManagedScaler
{
    Task<IEnumerable<ScaleResult>> ScaleFiles();
}
