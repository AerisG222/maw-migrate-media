namespace MawMediaMigrate;

static class StringExtensions
{
    public static string FixupMediaDirectory(this string path) => path
        .Replace(" ", "-")
        .Replace("_", "-");
}
