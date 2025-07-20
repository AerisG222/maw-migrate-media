namespace MawMediaMigrate.Scale;

interface IInspector
{
    Task BulkLoadSourceDimensions(string rootPath);
    Task<InspectResult> QueryDimensions(string path);
}
