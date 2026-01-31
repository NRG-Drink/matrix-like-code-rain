using ModularPipelines.Git.Models;

namespace NRG.Matrix.Build.Modules.Common;

public class GitVersionSettings
{
    public Func<GitVersionInformation, string?> VersionFunc { get; set; }
        = e => "main" == e.BranchName ? e.MajorMinorPatch : $"{e.MajorMinorPatch}-{e.PreReleaseLabel}.{e.CommitsSinceVersionSource}";
}
