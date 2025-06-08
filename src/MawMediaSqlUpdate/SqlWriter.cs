using MawMediaMigrate.Results;

namespace MawMediaSqlUpdate;

class SqlWriter
{
    const string SQL_EXIF_FILE = "update_exif.sql";
    const string SQL_SCALE_FILE = "update_scale.sql";
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

        await WriteExifSqlFile(mediaInfos);
        await WriteScaleSqlFile(mediaInfos);
        await WriteRunnerScript();
    }

    async Task WriteExifSqlFile(IEnumerable<MediaInfo> mediaInfos)
    {
        using var writer = new StreamWriter(Path.Combine(_outputDir.FullName, SQL_EXIF_FILE));

        // no pre/post amble as it seems to break using \set
        await WriteExifBody(writer, mediaInfos);

        await writer.FlushAsync();
        writer.Close();
    }

    async Task WriteScaleSqlFile(IEnumerable<MediaInfo> mediaInfos)
    {
        using var writer = new StreamWriter(Path.Combine(_outputDir.FullName, SQL_SCALE_FILE));

        await WritePreamble(writer);
        await WriteScaleBody(writer, mediaInfos);
        await WritePostamble(writer);

        await writer.FlushAsync();
        writer.Close();
    }

    async Task WriteExifBody(StreamWriter writer, IEnumerable<MediaInfo> mediaInfos)
    {
        foreach (var mi in mediaInfos)
        {
            await WriteExifMediaInfo(writer, mi);
        }
    }

    async Task WriteScaleBody(StreamWriter writer, IEnumerable<MediaInfo> mediaInfos)
    {
        foreach (var mi in mediaInfos)
        {
            await WriteScaleMediaInfo(writer, mi);
        }
    }

    async Task WriteExifMediaInfo(StreamWriter writer, MediaInfo mi)
    {
        await WriteExifUpdate(writer, mi);
    }

    async Task WriteScaleMediaInfo(StreamWriter writer, MediaInfo mi)
    {
        await WriteScaleUpdates(writer, mi);
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

            """
        );
    }

    async Task WriteRunnerScript()
    {
        var outfile = Path.Combine(_outputDir.FullName, "import.sh");
        using var writer = new StreamWriter(outfile);

        await writer.WriteLineAsync("#!/bin/bash");
        await writer.WriteLineAsync("");
        await WritePodmanExecuteScript(writer, SQL_EXIF_FILE);
        await writer.WriteLineAsync("");
        await WritePodmanExecuteScript(writer, SQL_SCALE_FILE);

        await writer.FlushAsync();
        writer.Close();
    }

    async Task WritePodmanExecuteScript(StreamWriter writer, string importFile)
    {
        await writer.WriteLineAsync(
            $"""
            psql \
                --host localhost \
                --port 6543 \
                --username svc_maw_media \
                --dbname maw_media \
                --file "{importFile}"
            """);
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
