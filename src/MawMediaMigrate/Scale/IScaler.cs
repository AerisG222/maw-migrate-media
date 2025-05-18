namespace MawMediaMigrate.Scale;

public interface IScaler
{
    Task<ScaleResult> Scale(FileInfo src);
}