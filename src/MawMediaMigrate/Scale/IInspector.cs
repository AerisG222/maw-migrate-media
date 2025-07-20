interface IInspector
{
    Task BulkLoadSourceDimensions(string rootPath);
    Task<(int width, int height)> QueryDimensions(string path);
}
