using System.Diagnostics;
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
        var results = new List<ScaledFile>();
        var (srcWidth, srcHeight) = await _inspector.QueryDimensions(src.FullName);
        var scales = GetScalesForDimensions(srcWidth, srcHeight, false);

        foreach (var scale in scales)
        {
            var dst = new FileInfo(
                Path.Combine(
                    src.Directory!.Parent!.FullName.Replace(origMediaRoot.FullName, _destRootDir.FullName).FixupMediaDirectory(),
                    scale.Code,
                    $"{Path.GetFileNameWithoutExtension(src.Name)}.avif"
                )
            );

            CreateDir(dst.DirectoryName!);

            var psi = new ProcessStartInfo
            {
                FileName = "magick",
                Arguments = GetImageMagickArgs(src.FullName, dst.FullName, scale),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = new Process
            {
                StartInfo = psi
            };

            process.Start();
            await process.WaitForExitAsync();

            var (scaledWidth, scaledHeight) = await _inspector.QueryDimensions(dst.FullName);

            lock (_lockObj)
            {
                results.Add(new ScaledFile(scale, dst.FullName, scaledWidth, scaledHeight, dst.Length));
            }
        }

        return new ScaleResult(src.FullName, results);
    }

    // https://usage.imagemagick.org/resize/
    static string GetImageMagickArgs(string src, string dst, ScaleSpec scale)
    {
        List<string> args = [
            $"\"{src}\""
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
                    "-resize", $"\"{scale.Width}x{scale.Height}>\""
                ]);
            }
        }

        args.AddRange([
            "-colorspace", "sRGB",
            "-quality", "72",
            "-strip",
            $"\"{dst}\""
        ]);

        return string.Join(" ", args);
    }
}
