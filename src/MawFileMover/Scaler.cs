using System.Diagnostics;

namespace MawFileMover;

public class Scaler
{
    List<Scale> _scales = [
        new ("qqvg", 160, 120, false),
        new ("qqvg-fill", 160, 120, true),
        new ("qvg", 320, 240, false),
        new ("qvg-fill", 320, 240, true),
        new ("nhd", 640, 480, false),
        new ("full_hd", 1920, 1080, false),
        new ("qhd", 2560, 1440, false),
        new ("4k", 3840, 2160, false)
    ];

    public void ScaleImage(FileInfo src)
    {
        Parallel.ForEach(_scales, scale =>
        {
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
            process.WaitForExit();
        });
    }

    public void ScaleVideo(FileInfo src)
    {
        // todo
    }

    string GetImageMagickArgs(string src, string dst, Scale scale)
    {
        List<string> args = [
            $"\"{src}\"",
            "-colorspace", "RGB"
        ];

        if (scale.FillsDimensions)
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

    void SafeCreateDir(string dir)
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
