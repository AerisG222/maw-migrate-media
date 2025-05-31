using MawMediaMigrate.Results.Reader;
using MawMediaSqlUpdate;

var opts = Options.FromArgs(args);

(var moveSpecs, var exifResults, var scaledFiles) = await new ResultReader().ReadResults(opts.SrcDir);

foreach(var moveSpec in moveSpecs)
{
    Console.WriteLine($"Move: {moveSpec.Src} -> {moveSpec.Dst}");
}
