using MawMediaMigrate.Results;

namespace MawMediaMigrate.Scale;

interface IScaler
{
    Task<ScaleResult> Scale(FileInfo src, DirectoryInfo origMediaRoot);
}
