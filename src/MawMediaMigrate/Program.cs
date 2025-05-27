using MawMediaMigrate;
using MawMediaMigrate.Exif;
using MawMediaMigrate.Move;
using MawMediaMigrate.Scale;
using MawMediaMigrate.Writer;

var opts = Options.FromArgs(args);

var mover = MoverFactory.Create(opts);
var exifExporter = ExifExporterFactory.Create(opts);
var scaler = ManagedScalerFactory.Create(opts);
var writer = ResultWriterFactory.Create(opts);

// consider dictionary w/ alt key to easily update records/results
var moveResults = mover.MoveFiles();
var exifResults = await exifExporter.ExportExifData(opts.DestDir);
var scaledFiles = await scaler.ScaleFiles();

await writer.WriteMappingFile(opts.MappingFile.FullName, moveResults);
