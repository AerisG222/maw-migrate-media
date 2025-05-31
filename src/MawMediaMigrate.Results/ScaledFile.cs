namespace MawMediaMigrate.Results;

public record class ScaledFile (
    ScaleSpec Scale,
    string Path
);
