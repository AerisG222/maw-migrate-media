using System.Diagnostics;

namespace MawFileMover;

public class Scaler
{
    object _lockObj = new();
    readonly Inspector _inspector = new();
    readonly List<Scale> _scales = [
        new ("qqvg", 160, 120, false),
        new ("qqvg-fill", 160, 120, true),
        new ("qvg", 320, 240, false),
        new ("qvg-fill", 320, 240, true),
        new ("nhd", 640, 480, false),
        new ("full_hd", 1920, 1080, false),
        new ("qhd", 2560, 1440, false),
        new ("4k", 3840, 2160, false)
    ];

    public async Task ScaleImage(FileInfo src)
    {
        var (srcWidth, srcHeight) = await _inspector.QueryDimensions(src.FullName);

        await Parallel.ForEachAsync(_scales, async (scale, token) =>
        {
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

            SafeCreateDir(dst.DirectoryName!);

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
        });
    }

    public async Task ScaleVideo(FileInfo src)
    {
        var (srcWidth, srcHeight) = await _inspector.QueryDimensions(src.FullName);

        await Parallel.ForEachAsync(_scales, async (scale, token) =>
        {
            if (!ShouldScale(srcWidth, srcHeight, scale))
            {
                lock (_lockObj)
                {
                    Console.WriteLine($"  - not scaling {src.Name} to {scale}");
                }

                return;
            }

            var dst = Path.Combine(src.Directory!.Parent!.FullName, scale.Code, Path.GetFileNameWithoutExtension(src.Name));
            var dstMovie = new FileInfo($"{dst}.mp4");
            var dstPoster = new FileInfo($"{dst}.poster.avif");
            List<(FileInfo dstFile, bool isPoster)> queue = [(dstMovie, false), (dstPoster, true)];

            SafeCreateDir(dstMovie.DirectoryName!);

            foreach(var (dstFile, isPoster) in queue)
            {
                var psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = GetFfmpegArgs(src.FullName, dstFile.FullName, scale, isPoster),
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
            }
        });
    }

    // https://usage.imagemagick.org/resize/
    static string GetImageMagickArgs(string src, string dst, Scale scale)
    {
        List<string> args = [
            $"\"{src}\"",
            "-colorspace", "RGB"
        ];

        if (scale.IsPreview)
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

    // https://trac.ffmpeg.org/wiki/Encode/AV1#SVT-AV1
    // https://www.ffmpeg.org/ffmpeg-all.html#scale-1
    static string GetFfmpegArgs(string src, string dst, Scale scale, bool isPoster)
    {
        List<string> args = [
            "-i", $"\"{src}\""
        ];

        if (scale.IsPreview)
        {
            // scale video to fit area (full height or width, with borders as necessary)
            //"-vf", $"\"scale={scale.Width}:{scale.Height}:force_original_aspect_ratio=decrease,pad={scale.Width}:{scale.Height}:(ow-iw)/2:(oh-ih)/2\"",

            // scale video to fit area (cropped to fit), drop sound, 24fps
            args.AddRange([
                "-an",
                "-vf", $"\"scale=(iw*sar)*max({scale.Width}/(iw*sar)\\,{scale.Height}/ih):ih*max({scale.Width}/(iw*sar)\\,{scale.Height}/ih), crop={scale.Width}:{scale.Height}\"",
                "-r", "24"
            ]);
        }
        else
        {
            args.AddRange([
                "-c:a", "aac",
                "-b:a", "128k",
                "-vf", $"\"scale={scale.Width}:{scale.Height}:force_original_aspect_ratio=decrease:force_divisible_by=2\""
            ]);
        }

        if (isPoster)
        {
            args.AddRange([
                "-ss", "00:00:02",
                "-frames:v", "1"
            ]);
        }

        args.AddRange([
            "-c:v", "libsvtav1",
            "-movflags", "+faststart",
            $"\"{dst}\""
        ]);

        return string.Join(" ", args);
    }

    static bool ShouldScale(int width, int height, Scale scale)
    {
        return width >= scale.Width || height >= scale.Height;
    }

    static void SafeCreateDir(string dir)
    {
        if (!Directory.Exists(dir))
        {
            try
            {
                Directory.CreateDirectory(dir);
            }
            catch
            {
                // swallow
            }
        }
    }
}
