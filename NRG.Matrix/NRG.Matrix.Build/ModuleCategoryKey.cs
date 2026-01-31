namespace NRG.Matrix.Build;

public static class ModuleCategoryKey
{
    public static string[] AllCategories => [Build, Test, TestReport, Pack, UploadNuGet, UploadNuGetStore, Publish, Release, UploadReleaseAssets, Container];

    public const string Container = "container";
    public const string UploadReleaseAssets = "upload-release-assets";
    public const string Release = "release";
    public const string Publish = "publish";
    public const string UploadNuGet = "upload-nuget";
    public const string UploadNuGetStore = "upload-nuget-store";
    public const string Pack = "pack";
    public const string TestReport = "test-report";
    public const string Test = "test";
    public const string Build = "build";
}
