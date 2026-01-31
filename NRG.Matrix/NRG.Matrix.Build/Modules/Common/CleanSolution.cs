using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.Models;
using ModularPipelines.Modules;

namespace NRG.Matrix.Build.Modules.Common;

[RunOnLocalMachineOnly]
[DependsOn<FindRepoPaths>]
[NotInParallel(NotInParallelKey.BuildExecute)]
public class CleanSolution : Module<CommandResult>
{
    protected override async Task<CommandResult?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var module = await context.GetModule<FindRepoPaths>();
        return await context.DotNet().Clean(
            options: new() { ProjectSolution = module.ValueOrDefault!.Solution, Configuration = "Release" },
            cancellationToken: cancellationToken);
    }
}
