using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.Models;
using ModularPipelines.Modules;

namespace NRG.Matrix.Build.Modules.Common;

[ModuleCategory(ModuleCategoryKey.Build)]
[DependsOn<FindRepoPaths>]
[DependsOn<CleanSolution>(Optional = true)]
[NotInParallel(NotInParallelKey.BuildExecute)]
public class RestoreSolution : Module<CommandResult>
{
    protected override async Task<CommandResult?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var module = await context.GetModule<FindRepoPaths>();
        return await context.DotNet().Restore(
            options: new()
            {
                Arguments = [module.ValueOrDefault!.Solution.Path],
            },
            cancellationToken: cancellationToken);
    }
}
