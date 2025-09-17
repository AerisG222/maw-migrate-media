namespace MawMediaMigrate.Results.Writer;

public interface IResultWriter
{
    Task WriteResults<T>(IEnumerable<T> results, string filename);
}
