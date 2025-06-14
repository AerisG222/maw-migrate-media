using MawMediaMigrate.Results;

namespace MawMediaSqlUpdate;

class SqlWriter
{
    const int RECORDS_PER_FILE = 10_000;
    readonly List<string> _sqlExifFiles = [];
    readonly List<string> _sqlScaleFiles = [];
    readonly DirectoryInfo _outputDir;
    readonly DirectoryInfo _destMediaDir;

    public SqlWriter(DirectoryInfo destMediaDir, DirectoryInfo outputDir)
    {
        ArgumentNullException.ThrowIfNull(destMediaDir);
        ArgumentNullException.ThrowIfNull(outputDir);

        _destMediaDir = destMediaDir;
        _outputDir = outputDir;
    }

    public async Task GenerateSql(IEnumerable<MediaInfo> mediaInfos)
    {
        _outputDir.Create();

        await WriteSqlFiles(mediaInfos);
        await WriteRunnerScript();
    }

    async Task WriteSqlFiles(IEnumerable<MediaInfo> mediaInfos)
    {
        var id = 0;
        var fileId = 0;
        StreamWriter? exifWriter = null;
        StreamWriter? scaleWriter = null;

        foreach (var mi in mediaInfos)
        {
            if (id % RECORDS_PER_FILE == 0)
            {
                fileId++;

                exifWriter = await PrepareExifWriter(exifWriter, fileId);
                scaleWriter = await PrepareScaleWriter(scaleWriter, fileId);
            }

            await WriteExifUpdate(exifWriter!, mi);
            await WriteScaleUpdates(scaleWriter!, mi);

            id++;
        }

        await TerminateExifWriter(exifWriter);
        await TerminateScaleWriter(scaleWriter);
    }

    async Task<StreamWriter> PrepareExifWriter(StreamWriter? sw, int fileId)
    {
        if (sw != null)
        {
            await TerminateWriter(sw, false);
        }

        var exifFilename = Path.Combine(_outputDir.FullName, $"exif_{fileId:00#}.sql");

        _sqlExifFiles.Add(exifFilename);

        return new StreamWriter(exifFilename);
    }

    async Task TerminateExifWriter(StreamWriter? sw)
    {
        if (sw != null)
        {
            await TerminateWriter(sw, false);
        }
    }

    async Task<StreamWriter> PrepareScaleWriter(StreamWriter? sw, int fileId)
    {
        if (sw != null)
        {
            await TerminateWriter(sw, true);
        }

        var scaleFilename = Path.Combine(_outputDir.FullName, $"scale_{fileId:00#}.sql");

        _sqlScaleFiles.Add(scaleFilename);

        sw = new StreamWriter(scaleFilename);

        await WritePreamble(sw);

        await sw.WriteLineAsync(
            $"""
            CREATE TABLE IF NOT EXISTS media.tmpmediafile
            (
                srcpath TEXT,
                typename TEXT,
                scalecode TEXT,
                width INTEGER,
                height INTEGER,
                bytes BIGINT,
                path TEXT
            );

            """
        );

        return sw;
    }

    async Task TerminateScaleWriter(StreamWriter? sw)
    {
        if (sw != null)
        {
            await TerminateWriter(sw, true);
        }
    }

    async Task TerminateWriter(StreamWriter sw, bool addPostamble)
    {
        if (addPostamble)
        {
            await WritePostamble(sw);
        }

        await sw.FlushAsync();
        sw.Close();
        sw.Dispose();
    }

    async Task WriteExifUpdate(StreamWriter writer, MediaInfo mi)
    {
        if (mi.ExifFile == null)
        {
            return;
        }

        var path = mi.DestinationSrcPath.Replace(_destMediaDir.Parent!.FullName, string.Empty);

        await writer.WriteLineAsync(
            $"""
            \set media_metadata `cat {mi.ExifFile}`

            UPDATE media.media
                SET metadata = :'media_metadata'::jsonb
                WHERE id = (
                    SELECT f.media_id
                    FROM media.media_file f
                    INNER JOIN media.scale s
                        ON s.id = f.scale_id
                        AND s.code = 'src'
                        AND f.path = '{path}'
                );

            """
        );
    }

    async Task WriteScaleUpdates(StreamWriter writer, MediaInfo mi)
    {
        foreach (var scale in mi.ScaledFiles)
        {
            await WriteScaleUpdate(writer, mi, scale);
        }
    }

    async Task WriteScaleUpdate(StreamWriter writer, MediaInfo mi, ScaledFile file)
    {
        var srcPath = mi.DestinationSrcPath.Replace(_destMediaDir.Parent!.FullName, string.Empty);
        var scalePath = file.Path.Replace(_destMediaDir.Parent!.FullName, string.Empty);
        var typeName = file.Scale.IsPoster
            ? "video-poster"
            : string.Equals(Path.GetExtension(file.Path), ".avif", StringComparison.OrdinalIgnoreCase)
                ? "photo"
                : "video";

        await writer.WriteLineAsync(
            $"""
            INSERT INTO media.tmpmediafile
            (
                srcpath,
                typename,
                scalecode,
                width,
                height,
                bytes,
                path
            )
            VALUES
            (
                '{srcPath}',
                '{typeName}',
                '{file.Scale.Code}',
                {file.Width},
                {file.Height},
                {file.Bytes},
                '{scalePath}'
            );

            """
        );
    }

    async Task WriteRunnerScript()
    {
        var outfile = Path.Combine(_outputDir.FullName, "import.sh");
        using var writer = new StreamWriter(outfile);

        await writer.WriteLineAsync("#!/bin/bash");
        await writer.WriteLineAsync("");
        await writer.WriteLineAsync("export PGPASSWORD='<SET_PWD_HERE>'");
        await writer.WriteLineAsync("");
        await writer.WriteLineAsync("export PGHOST=localhost");
        await writer.WriteLineAsync("export PGPORT=6543");
        await writer.WriteLineAsync("export PGDATABASE=maw_media");
        await writer.WriteLineAsync("export PGUSER=postgres");
        await writer.WriteLineAsync("");

        await WriteScripts(writer, _sqlExifFiles);
        await writer.WriteLineAsync("");
        await WriteScripts(writer, _sqlScaleFiles);

        await writer.FlushAsync();
        writer.Close();
    }

    async Task WriteScripts(StreamWriter writer, IEnumerable<string> files)
    {
        foreach (var file in files)
        {
            await WritePodmanExecuteScript(writer, file);
        }
    }


    async Task WritePodmanExecuteScript(StreamWriter writer, string importFile)
    {
        await writer.WriteLineAsync($"psql --file '{importFile}'");
    }

    async Task WritePreamble(StreamWriter writer)
    {
        await writer.WriteLineAsync(
            """
            DO
            $$
            BEGIN

            """
        );
    }

    async Task WritePostamble(StreamWriter writer)
    {
        await writer.WriteLineAsync(
            """
            END
            $$

            """
        );
    }
}
