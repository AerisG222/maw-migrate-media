namespace MawMediaMigrate.Results;

public record class ScaledFile (
    ScaleSpec Scale,
    string Path,
    int Width,
    int Height,
    long Bytes
);
