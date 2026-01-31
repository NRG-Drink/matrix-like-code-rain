using NRG.Matrix.Build.Modules.Packing;
using ModularPipelines.Attributes;
using ModularPipelines.Configuration;
using ModularPipelines.Context;
using ModularPipelines.Models;
using ModularPipelines.Modules;

namespace NRG.Matrix.Build.Modules.Common;

[RunOnLocalMachineOnly]
[RunOnWindows]
[DependsOn<FindRepoPaths>]
[DependsOn<PackNuGet>]
[NotInParallel(nameof(OpenArtifactsFolder))]
public class OpenArtifactsFolder : Module<CommandResult>
{
    protected override ModuleConfiguration Configure()
        => ModuleConfiguration.Create()
            .WithIgnoreFailures()
            .Build();

    protected override async Task<CommandResult?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var repoPaths = await context.GetModule<FindRepoPaths>();
        var outDir = repoPaths.ValueOrDefault!.Artifacts;
        return await context.Shell.PowerShell.Script(new("explorer") { Arguments = [outDir.Path] }, cancellationToken);
    }
}
