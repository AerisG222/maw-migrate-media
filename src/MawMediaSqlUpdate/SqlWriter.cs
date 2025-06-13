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
                if (exifWriter != null)
                {
                    await exifWriter.FlushAsync();
                    exifWriter.Close();
                    exifWriter.Dispose();
                }

                if (scaleWriter != null)
                {
                    await WritePostamble(scaleWriter);
                    await scaleWriter.FlushAsync();
                    scaleWriter.Close();
                    scaleWriter.Dispose();
                }

                fileId++;

                var exifFilename = Path.Combine(_outputDir.FullName, $"exif_{fileId:00#}.sql");
                _sqlExifFiles.Add(exifFilename);
                exifWriter = new StreamWriter(exifFilename);

                var scaleFilename = Path.Combine(_outputDir.FullName, $"scale_{fileId:00#}.sql");
                _sqlScaleFiles.Add(scaleFilename);
                scaleWriter = new StreamWriter(scaleFilename);
                await WritePreamble(scaleWriter);
            }

            await WriteExifUpdate(exifWriter!, mi);
            await WriteScaleUpdates(scaleWriter!, mi);

            id++;
        }
    }

    async Task WriteExifUpdate(StreamWriter writer, MediaInfo mi)
    {
        if (mi.ExifFile == null)
        {
            return;
        }

        var path = mi.DestinationSrcPath.Replace(_destMediaDir.Parent!.FullName, string.Empty);

        await writer.WriteLineAsync($"""
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

        await writer.WriteLineAsync($"""
            IF EXISTS (
                SELECT f.media_id
                    FROM media.media_file f
                    INNER JOIN media.scale s
                        ON s.id = f.scale_id
                        AND s.code = 'src'
                        AND f.path = '{srcPath}'
            ) THEN
                INSERT INTO media.media_file
                (
                    media_id,
                    media_type_id,
                    scale_id,
                    width,
                    height,
                    bytes,
                    path
                )
                VALUES
                (
                    (
                        SELECT f.media_id
                        FROM media.media_file f
                        INNER JOIN media.scale s
                            ON s.id = f.scale_id
                            AND s.code = 'src'
                            AND f.path = '{srcPath}'
                    ),
                    (
                        SELECT id
                        FROM media.media_type
                        WHERE name  = '{typeName}'
                    ),
                    (
                        SELECT id
                        FROM media.scale
                        WHERE code = '{file.Scale.Code}'
                    ),
                    {file.Width},
                    {file.Height},
                    {file.Bytes},
                    '{scalePath}'
                );
            ELSE
                RAISE NOTICE 'src not found: {srcPath}';
            END IF;

            """
        );
    }

    async Task WriteRunnerScript()
    {
        var outfile = Path.Combine(_outputDir.FullName, "import.sh");
        using var writer = new StreamWriter(outfile);

        await writer.WriteLineAsync("#!/bin/bash");
        await writer.WriteLineAsync("");
        await writer.WriteLineAsync("export PGPASSWORD=");
        await writer.WriteLineAsync("");
        await writer.WriteLineAsync("export PGHOST=localhost");
        await writer.WriteLineAsync("export PGPORT=6543");
        await writer.WriteLineAsync("export PGDATABASE=maw_media");
        await writer.WriteLineAsync("export PGUSER=svc_maw_media");
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

            """);
    }

    async Task WritePostamble(StreamWriter writer)
    {
        await writer.WriteLineAsync(
            """
            END
            $$

            """);
    }
}
