using MawMediaMigrate.Results;

namespace MawMediaMigrate.Scale;

class DryRunPhotoScaler
    : BaseScaler
{
    public DryRunPhotoScaler(IInspector inspector, DirectoryInfo origRootDir, DirectoryInfo destRootDir)
        : base(inspector, origRootDir, destRootDir)
    {

    }

    public override async Task<ScaleResult> Scale(FileInfo src, DirectoryInfo origMediaRoot)
    {
        var results = new List<ScaledFile>();
        var (srcWidth, srcHeight) = await _inspector.QueryDimensions(src.FullName);

        foreach(var scale in ScaleSpec.AllScales)
        {
            if (scale.IsPoster)
            {
                continue;  // only for videos
            }

            if (!ShouldScale(srcWidth, srcHeight, scale))
            {
                continue;
            }

            var dst = new FileInfo(
                Path.Combine(
                    src.Directory!.Parent!.FullName.Replace(origMediaRoot.FullName, _destRootDir.FullName).FixupMediaDirectory(),
                    scale.Code,
                    $"{Path.GetFileNameWithoutExtension(src.Name)}.avif"
                )
            );

            lock (_lockObj)
            {
                results.Add(new ScaledFile(scale, dst.FullName));
            }
        }

        return new ScaleResult(src.FullName, results);
    }
}
