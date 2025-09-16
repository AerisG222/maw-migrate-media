using MawMediaMigrate;
using MawMediaMigrate.Exif;
using MawMediaMigrate.Move;
using MawMediaMigrate.Scale;
using MawMediaMigrate.Results.Writer;
using MawMediaMigrate.VideoDuration;

var opts = Options.FromArgs(args);

// var scaleProcessor = new ScaleProcessor(opts);
// var moveProcessor = new MoveProcessor(opts);
// var exifProcessor = new ExifProcessor(opts);
var durationProcessor = new DurationProcessor(opts);
var writer = new ResultWriter(opts.OutDir);

// var scaledFiles = await scaleProcessor.ScaleFiles();
// var moveResults = await moveProcessor.MoveFiles();
// var exifResults = await exifProcessor.ExportExifData();
var durationResults = await durationProcessor.InspectFiles();

// await writer.WriteResults(moveResults, exifResults, scaledFiles);

await writer.WriteDurationResults(durationResults);
