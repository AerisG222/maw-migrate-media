namespace MawMediaMigrate.Scale;

record class ScaleResult (
    string SrcPath,
    List<ScaledFile> ScaledFiles
);
