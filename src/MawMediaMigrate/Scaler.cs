using System.Diagnostics;

namespace MawMediaMigrate;

public class Scaler
{
    readonly Lock _lockObj = new();
    readonly Inspector _inspector = new();
    readonly List<Scale> _scales = [
        new ("qqvg",             160,  120, false, false),
        new ("qqvg-poster",      160,  120, false, true),
        new ("qqvg-fill",        160,  120, true,  false),
        new ("qqvg-fill-poster", 160,  120, true,  true),
        new ("qvg",              320,  240, false, false),
        new ("qvg-poster",       320,  240, false, true),
        new ("qvg-fill",         320,  240, true,  false),
        new ("qvg-fill-poster",  320,  240, true,  true),
        new ("nhd",              640,  480, false, false),
        new ("nhd-poster",       640,  480, false, true),
        new ("full_hd",         1920, 1080, false, false),
        new ("full_hd-poster",  1920, 1080, false, true),
        new ("qhd",             2560, 1440, false, false),
        new ("qhd-poster",      2560, 1440, false, true),
        new ("4k",              3840, 2160, false, false),
        new ("4k-poster",       3840, 2160, false, true)
    ];

    public async Task<IEnumerable<ScaledFile>> ScaleImage(FileInfo src)
    {
        var results = new List<ScaledFile>();
        var (srcWidth, srcHeight) = await _inspector.QueryDimensions(src.FullName);

        await Parallel.ForEachAsync(_scales, async (scale, token) =>
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

            lock (_lockObj)
            {
                results.Add(new ScaledFile(scale, dst.FullName));
            }
        });

        return results;
    }

    public async Task<IEnumerable<ScaledFile>> ScaleVideo(FileInfo src)
    {
        var results = new List<ScaledFile>();
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

            var dstPrefix = Path.Combine(src.Directory!.Parent!.FullName, scale.Code, Path.GetFileNameWithoutExtension(src.Name));
            var dstFile = new FileInfo($"{dstPrefix}{(scale.IsPoster ? ".poster.avif" : ".mp4")}");

            SafeCreateDir(dstFile.DirectoryName!);

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = GetFfmpegArgs(src.FullName, dstFile.FullName, scale),
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
                results.Add(new ScaledFile(scale, dstFile.FullName));
            }
        });

        return results;
    }

    // https://usage.imagemagick.org/resize/
    static string GetImageMagickArgs(string src, string dst, Scale scale)
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

    // https://trac.ffmpeg.org/wiki/Encode/AV1#SVT-AV1
    // https://www.ffmpeg.org/ffmpeg-all.html#scale-1
    static string GetFfmpegArgs(string src, string dst, Scale scale)
    {
        List<string> args = [
            "-i", $"\"{src}\""
        ];

        if (scale.IsCropToFill)
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

        if (scale.IsPoster)
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
