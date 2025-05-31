namespace MawMediaMigrate.Results;

public record class ScaleResult (
    string SrcPath,
    List<ScaledFile> ScaledFiles
);
