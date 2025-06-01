using MawMediaMigrate.Results.Reader;
using MawMediaSqlUpdate;

var opts = Options.FromArgs(args);
var repo = new MediaRepo(opts.OrigMediaDir, opts.DestMediaDir);

(var moveSpecs, var exifResults, var scaledFiles) = await new ResultReader().ReadResults(opts.JsonResultsDir);

repo.AssembleMediaInfo(moveSpecs, exifResults,scaledFiles);

Console.WriteLine("a");
