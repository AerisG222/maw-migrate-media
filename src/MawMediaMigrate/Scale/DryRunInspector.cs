namespace MawMediaMigrate.Scale;

class DryRunInspector
    : IInspector
{
    public Task BulkLoadSourceDimensions(string rootPath)
    {
        return Task.CompletedTask;
    }

    public Task<InspectResult> QueryDimensions(string path)
    {
        return Task.FromResult(new InspectResult(path, 1920, 1080));
    }
}
