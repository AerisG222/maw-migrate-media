namespace MawMediaMigrate.Scale;

record class ScaleSpec(
    string Code,
    int Width,
    int Height,
    bool IsCropToFill,
    bool IsPoster
) {
    static readonly ScaleSpec[] _allScales = [
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

    public static ScaleSpec[] AllScales => _allScales;
}
