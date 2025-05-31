using MawMediaMigrate;
using MawMediaMigrate.Exif;
using MawMediaMigrate.Move;
using MawMediaMigrate.Scale;
using MawMediaMigrate.Results.Writer;

var opts = Options.FromArgs(args);

// note: dry-run currently doesn't work as some steps expect that directories are created / files are moved
// not going to build out the rest of that now as its not currently worth the time, but will leave here
// in case it becoms useful in the future =|

var scaleProcessor = new ScaleProcessor(opts);
var moveProcessor = new MoveProcessor(opts);
var exifProcessor = new ExifProcessor(opts);
var writer = new ResultWriter(opts.OutDir);

var scaledFiles = await scaleProcessor.ScaleFiles();
var moveResults = moveProcessor.MoveFiles();
var exifResults = await exifProcessor.ExportExifData();

await writer.WriteResults(moveResults, exifResults, scaledFiles);
