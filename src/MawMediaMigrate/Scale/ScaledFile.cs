namespace MawMediaMigrate.Scale;

public record class ScaledFile (
    ScaleSpec Scale,
    string Path
);
