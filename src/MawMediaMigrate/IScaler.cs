namespace MawMediaMigrate;

public interface IScaler
{
    Task<IEnumerable<ScaledFile>> Scale(FileInfo src);
}