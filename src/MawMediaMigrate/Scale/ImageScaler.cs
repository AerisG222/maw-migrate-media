using System.Diagnostics;

namespace MawMediaMigrate.Scale;

public class ImageScaler
    : BaseScaler
{
    readonly DirectoryInfo _origDir;
    readonly DirectoryInfo _destDir;

    public ImageScaler(DirectoryInfo origDir, DirectoryInfo destDir)
    {
        _origDir = origDir;
        _destDir = destDir;
    }

    public override async Task<ScaleResult> Scale(FileInfo src)
    {
        var results = new List<ScaledFile>();
        var (srcWidth, srcHeight) = await _inspector.QueryDimensions(src.FullName);

        await Parallel.ForEachAsync(ScaleSpec.AllScales, async (scale, token) =>
        {
            if (scale.IsPoster)
            {
                return;  // only for videos
            }

            if (!ShouldScale(srcWidth, srcHeight, scale))
            {
                lock (_lockObj)
                {
                    Console.WriteLine($"  - not scaling {src.Name} to {scale}");
                }

                return;
            }

            var dst = new FileInfo(
                Path.Combine(src.Directory!.Parent!.FullName, scale.Code, $"{Path.GetFileNameWithoutExtension(src.Name)}.avif")
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

            lock (_lockObj)
            {
                results.Add(new ScaledFile(scale, dst.FullName));
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
            args.AddRange([
                "-resize", $"{scale.Width}x{scale.Height}"
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
