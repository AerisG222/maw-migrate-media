using MawMediaMigrate;

var opts = Options.FromArgs(args);

var mover = new Mover(opts.OrigDir, opts.DestDir);
var writer = new ResultWriter();

var results = mover.MoveFiles();

await writer.WriteMappingFile(opts.MappingFile.FullName, results);
