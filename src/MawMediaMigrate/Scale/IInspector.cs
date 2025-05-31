interface IInspector
{
    Task<(int width, int height)> QueryDimensions(string path);
}
