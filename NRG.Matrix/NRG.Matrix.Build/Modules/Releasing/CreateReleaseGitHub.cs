using NRG.Matrix.Build.Modules.Common;
using NRG.Matrix.Build.Modules.Packing;
using NRG.Matrix.Build.Modules.Publishing;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.Git.Extensions;
using ModularPipelines.GitHub.Extensions;
using ModularPipelines.Modules;
using Octokit;

namespace NRG.Matrix.Build.Modules.Releasing;

[ModuleCategory(ModuleCategoryKey.Release)]
[RunOnServerOnly]
[DependsOn<FindRepoPaths>]
[DependsOn<GitVersion>]
[DependsOn<PackNuGet>(Optional = true)]
[DependsOn<Publish>(Optional = true)]
[NotInParallel(NotInParallelKey.Release)]
public class CreateReleaseGitHub : Module<Release>
{
    protected override async Task<Release?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var versionModule = await context.GetModule<GitVersion>();
        var gitHub = context.GitHub();
        var version = $"v{versionModule.ValueOrDefault}";

        // TODO: Make release configurable
        var release = new NewRelease(version)
        {
            Name = version,
            Draft = true,
            Prerelease = context.Git().Information.BranchName is var b && !(b == "main" || b == "master"),
            GenerateReleaseNotes = true,
            TargetCommitish = context.Git().Information.LastCommitSha,
        };

        var result = new Release();
        if (long.TryParse(gitHub.EnvironmentVariables.RepositoryId, out var repoIdLong))
        {
            return await gitHub.Client.Repository.Release.Create(repoIdLong, release);
        }

        return await gitHub.Client.Repository.Release.Create(gitHub.RepositoryInfo.Owner, gitHub.RepositoryInfo.RepositoryName, release);
        //context.LogOnPipelineEnd($"✅🚩 Successfully Release Created {result.Name}");

        //return result;
    }
}
