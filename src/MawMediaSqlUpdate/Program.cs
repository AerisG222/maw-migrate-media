using MawMediaMigrate.Results.Reader;
using MawMediaSqlUpdate;

var opts = Options.FromArgs(args);
var repo = new MediaRepo(opts.OrigMediaDir);
var writer = new SqlWriter(opts.DestMediaDir, opts.SqlOutputDir);

(
    var moveSpecs,
    var exifResults,
    var scaledFiles,
    var durationResults
) = await new ResultReader().ReadResults(opts.JsonResultsDir);

repo.AssembleMediaInfo(moveSpecs, exifResults, scaledFiles, durationResults);

await writer.GenerateSql(repo.GetMediaInfos());
