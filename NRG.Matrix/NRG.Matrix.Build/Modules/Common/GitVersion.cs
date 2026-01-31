using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.Git.Extensions;
using ModularPipelines.Modules;

namespace NRG.Matrix.Build.Modules.Common;

[ModuleCategory(ModuleCategoryKey.Build)]
[NotInParallel(nameof(GitVersion))]
public class GitVersion(IOptions<GitVersionSettings> versionSettings) : Module<string>
{
    protected override async Task<string?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var versionInfo = await context.Git().Versioning.GetGitVersioningInformation();
        var version = versionSettings.Value.VersionFunc(versionInfo)
            ?? throw new Exception("No version could be found");
        context.Logger.LogInformation("Picked version: '{Version}'", version);
        //context.LogOnPipelineEnd($"Generated Version Number: {version}");

        return version;
    }
}
