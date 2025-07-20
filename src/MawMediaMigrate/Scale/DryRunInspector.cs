class DryRunInspector
    : IInspector
{
    public Task BulkLoadSourceDimensions(string rootPath)
    {
        return Task.CompletedTask;
    }

    public Task<(int width, int height)> QueryDimensions(string path)
    {
        return Task.FromResult((1920, 1080));
    }
}
