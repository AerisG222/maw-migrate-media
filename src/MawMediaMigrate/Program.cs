using MawMediaMigrate;
using MawMediaMigrate.Exif;
using MawMediaMigrate.Move;
using MawMediaMigrate.Scale;
using MawMediaMigrate.Results.Writer;
using MawMediaMigrate.VideoDuration;

var opts = Options.FromArgs(args);

var writer = new ResultWriter(opts.OutDir);

var scaleProcessor = new ScaleProcessor(opts);
var scaledFiles = await scaleProcessor.ScaleFiles();
await writer.WriteResults(scaledFiles, "scale.json");

var moveProcessor = new MoveProcessor(opts);
var moveResults = await moveProcessor.MoveFiles();
await writer.WriteResults(moveResults, "move.json");

var exifProcessor = new ExifProcessor(opts);
var exifResults = await exifProcessor.ExportExifData();
await writer.WriteResults(exifResults, "exif.json");

var durationProcessor = new DurationProcessor(opts);
var durationResults = await durationProcessor.InspectFiles();
await writer.WriteResults(durationResults, "duration.json");
