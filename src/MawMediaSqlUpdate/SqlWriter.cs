namespace MawMediaSqlUpdate;

class SqlWriter
{
    const string SQL_FILE = "update_media.sql";
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

        await WriteSqlFile(mediaInfos);
        await WriteRunnerScript();
    }

    async Task WriteSqlFile(IEnumerable<MediaInfo> mediaInfos)
    {
        using var writer = new StreamWriter(Path.Combine(_outputDir.FullName, SQL_FILE));

        await WriteBody(writer, mediaInfos);

        await writer.FlushAsync();
        writer.Close();
    }

    async Task WriteBody(StreamWriter writer, IEnumerable<MediaInfo> mediaInfos)
    {
        foreach (var mi in mediaInfos)
        {
            await WriteMediaInfo(writer, mi);
        }
    }

    async Task WriteMediaInfo(StreamWriter writer, MediaInfo mi)
    {
        await WriteExifUpdate(writer, mi);
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

    async Task WriteRunnerScript()
    {
        var outfile = Path.Combine(_outputDir.FullName, "import.sh");
        using var writer = new StreamWriter(outfile);

        // currently we expect to run this directly on system where media was migrated as it needs
        // access to the generated json files...
        await writer.WriteLineAsync(
            $"""
            #!/bin/bash

            psql \
                --host localhost \
                --port 6543 \
                --username svc_maw_media \
                --dbname maw_media \
                --file "{SQL_FILE}"
            """);

        await writer.FlushAsync();
        writer.Close();
    }
}
