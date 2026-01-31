using ModularPipelines.FileSystem;
using MPFile = ModularPipelines.FileSystem.File;

namespace NRG.Matrix.Build;

public record RepoPaths
{
    public const string DebugToRepo = "..\\..\\..\\..";
    public static readonly string RunStart = $"{DateTime.Now:yyyy-MM-ddTHHmmss}";
    public required bool IsServer { get; init; }

    public required Folder Repo { get; init; } = new(".");
    public required MPFile Solution { get; init; } = new(".");

    public List<MPFile> ExeProjects { get; init; } = [];
    public List<MPFile> TestProjects { get; init; } = [];
    public List<MPFile> LibraryProjects { get; init; } = [];
    public List<MPFile> WinExeProjects { get; init; } = [];
    public List<MPFile> ModuleProjects { get; init; } = [];
    public List<MPFile> ContainerProjects { get; init; } = [];

    public Folder Artifacts => new(Path.Combine(Repo.Path, IsServer ? "artifacts" : ".artifacts"));
    // Test
    public Folder TestResultsBase => new(Path.Combine(Repo.Path, ".test-results"));
    public Folder TestHistory => new(Path.Combine(TestResultsBase.Path, $"_history"));
    public Folder TestResultsRun => new(Path.Combine(TestResultsBase.Path, $"report_{RunStart}"));
    public Folder TestResultsData => new(Path.Combine(TestResultsRun.Path, "data"));
    public Folder TestResultsReport => new(Path.Combine(TestResultsRun.Path, "report"));
    public MPFile TestReportHtml => new(Path.Combine(TestResultsReport.Path, "index.html"));
    public MPFile TestReportGitHubSummary => new(Path.Combine(TestResultsReport.Path, "SummaryGithub.md"));
    // Release
    public Folder ReleaseAssets => new(Path.Combine(Repo.Path, ".release-assets"));
}

