using System.Diagnostics;
using MawMediaMigrate.Results;

namespace MawMediaMigrate.Scale;

class VideoScaler
    : BaseScaler
{
    public VideoScaler(IInspector inspector, DirectoryInfo origRootDir, DirectoryInfo destRootDir)
        : base(inspector, origRootDir, destRootDir)
    {

    }

    public override async Task<ScaleResult> Scale(FileInfo src, DirectoryInfo origMediaRoot)
    {
        var results = new List<ScaledFile>();
        var (srcWidth, srcHeight) = await _inspector.QueryDimensions(src.FullName);
        var scales = GetScalesForDimensions(srcWidth, srcHeight, true);

        foreach(var scale in scales)
        {
            var dst = new FileInfo(
                Path.Combine(
                    src.Directory!.Parent!.FullName.Replace(origMediaRoot.FullName, _destRootDir.FullName).FixupMediaDirectory(),
                    scale.Code,
                    $"{Path.GetFileNameWithoutExtension(src.Name)}{(scale.IsPoster ? ".poster.avif" : ".mp4")}"
                )
            );

            // no need to rerun if it already exists - allows for resuming job
            if (dst.Exists)
            {
                CreateDir(dst.DirectoryName!);

                var psi = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = GetFfmpegArgs(src.FullName, dst.FullName, scale),
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
            }

            var (scaledWidth, scaledHeight) = await _inspector.QueryDimensions(dst.FullName);

            results.Add(new ScaledFile(scale, dst.FullName, scaledWidth, scaledHeight, dst.Length));
        };

        return new ScaleResult(src.FullName, results);
    }

    // https://trac.ffmpeg.org/wiki/Encode/AV1#SVT-AV1
    // https://www.ffmpeg.org/ffmpeg-all.html#scale-1
    // https://evilmartians.com/chronicles/better-web-video-with-av1-codec
    static string GetFfmpegArgs(string src, string dst, ScaleSpec scale)
    {
        List<string> args = [
            "-i", $"\"{src}\"",
            "-map_metadata", "-1"
        ];

        if (scale.Width != int.MaxValue)
        {
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
                    "-vf", $"\"scale='min({scale.Width},iw)':'min({scale.Height},ih)':force_original_aspect_ratio=decrease:force_divisible_by=2\""
                ]);
            }
        }
        else
        {
            args.AddRange([
                "-c:a", "aac",
                "-b:a", "128k",
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
}
