using MawMediaMigrate;
using MawMediaMigrate.Exif;
using MawMediaMigrate.Move;
using MawMediaMigrate.Scale;
using MawMediaMigrate.Writer;

var opts = Options.FromArgs(args);

var mover = new Mover(opts.OrigDir, opts.DestDir);
var exifExporter = new ExifExporter();
var scaler = new Scaler(opts.OrigDir, opts.DestDir);
var writer = new ResultWriter();

// consider dictionary w/ alt key to easily update records/results
var moveResults = mover.MoveFiles();
var exifResults = await exifExporter.ExportExifData(opts.DestDir);
var scaledFiles = await scaler.ScaleFiles();

await writer.WriteMappingFile(opts.MappingFile.FullName, moveResults);
