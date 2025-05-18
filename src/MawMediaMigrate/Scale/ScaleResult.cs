namespace MawMediaMigrate.Scale;

public record class ScaleResult (
    string SrcPath,
    List<ScaledFile> ScaledFiles
);
