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

            results.Add(new ScaledFile(scale, dst.FullName, 100, 200, 300));
        }

        return new ScaleResult(src.FullName, results);
    }
}
