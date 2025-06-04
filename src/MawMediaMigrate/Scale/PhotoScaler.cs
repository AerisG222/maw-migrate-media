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

        await Parallel.ForEachAsync(scales, async (scale, token) =>
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
            await process.WaitForExitAsync(token);

            var (scaledWidth, scaledHeight) = await _inspector.QueryDimensions(dst.FullName);

            lock (_lockObj)
            {
                results.Add(new ScaledFile(scale, dst.FullName, scaledWidth, scaledHeight, dst.Length));
            }
        });

        return new ScaleResult(src.FullName, results);
    }

    // https://usage.imagemagick.org/resize/
    static string GetImageMagickArgs(string src, string dst, ScaleSpec scale)
    {
        List<string> args = [
            $"\"{src}\"",
            "-colorspace", "RGB"
        ];

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

        args.AddRange([
            "-colorspace", "sRGB",
            "-quality", "72",
            $"\"{dst}\""
        ]);

        return string.Join(" ", args);
    }
}
