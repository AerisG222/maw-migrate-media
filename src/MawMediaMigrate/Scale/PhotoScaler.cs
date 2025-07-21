using CliWrap;
using MawMediaMigrate.Results;

namespace MawMediaMigrate.Scale;

class PhotoScaler
    : BaseScaler
{
    public PhotoScaler(IInspector inspector, DirectoryInfo origRootDir, DirectoryInfo destRootDir)
        : base(inspector, origRootDir, destRootDir)
    {

    }

    public override async Task<ScaleResult> Scale(FileInfo src, DirectoryInfo origMediaRoot)
    {
        InspectResult? scaledDims;
        var results = new List<ScaledFile>();
        InspectResult? srcDims = await _inspector.QueryDimensions(src.FullName);
        var scales = GetScalesForDimensions(srcDims.ImageWidth, srcDims.ImageHeight, false);

        foreach (var scale in scales)
        {
            var dst = new FileInfo(
                Path.Combine(
                    src.Directory!.Parent!.FullName.Replace(origMediaRoot.FullName, _destRootDir.FullName).FixupMediaDirectory(),
                    scale.Code,
                    $"{Path.GetFileNameWithoutExtension(src.Name)}.avif"
                )
            );

            if (!dst.Exists)
            {
                await ScaleImage(src, dst, scale);
            }

            try
            {
                scaledDims = await _inspector.QueryDimensions(dst.FullName);
            }
            catch
            {
                // when reprocessing, sometimes corrupt or zero length files were created, so if this is the case,
                // try to cleanup and rescale.  if this still fails, something funny is going on so log the file
                try
                {
                    File.Delete(dst.FullName);
                }
                catch
                {
                    // swallow
                }

                await ScaleImage(src, dst, scale);

                try
                {
                    scaledDims = await _inspector.QueryDimensions(dst.FullName);
                }
                catch
                {
                    Console.WriteLine($"  ** Unable to process {src.FullName}");
                    continue;
                }
            }

            dst.Refresh();
            results.Add(new ScaledFile(scale, dst.FullName, scaledDims.ImageWidth, scaledDims.ImageHeight, dst.Length));
        }

        return new ScaleResult(src.FullName, results);
    }

    static async Task ScaleImage(FileInfo src, FileInfo dst, ScaleSpec scale)
    {
        CreateDir(dst.DirectoryName!);

        using var cmd = Cli
            .Wrap("magick")
            .WithArguments(GetImageMagickArgs(src.FullName, dst.FullName, scale))
            .ExecuteAsync();

        await cmd;
    }

    // https://usage.imagemagick.org/resize/
    static IEnumerable<string> GetImageMagickArgs(string src, string dst, ScaleSpec scale)
    {
        List<string> args = [
            src
        ];

        if (scale.Width != int.MaxValue)
        {
            // interesting, if we include this above, magick consumes a lot of mem and crashes the process
            // when trying to process the 'full' size.  in this case, we don't try to use a giant size, but
            // looks like magick doesn't like 2 colorspace cmds side by side...
            args.AddRange([
                "-colorspace", "RGB"
            ]);

            if (scale.IsCropToFill)
            {
                args.AddRange([
                    "-resize", $"{scale.Width}x{scale.Height}^",
                    "-gravity", "center",
                    "-crop", $"{scale.Width}x{scale.Height}+0+0"
                ]);
            }
            else
            {
                // the > at the end means "only scale down, not up"
                args.AddRange([
                    "-resize", $"{scale.Width}x{scale.Height}>"
                ]);
            }
        }

        args.AddRange([
            "-colorspace", "sRGB",
            "-quality", "72",
            "-strip",
            dst
        ]);

        return args;
    }
}
