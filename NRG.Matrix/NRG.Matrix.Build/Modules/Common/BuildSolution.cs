using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.Models;
using ModularPipelines.Modules;

namespace NRG.Matrix.Build.Modules.Common;

[ModuleCategory(ModuleCategoryKey.Build)]
[DependsOn<FindRepoPaths>]
[DependsOn<RestoreSolution>(Optional = true)]
[NotInParallel(NotInParallelKey.BuildExecute)]
public class BuildSolution : Module<CommandResult>
{
    protected override async Task<CommandResult?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var module = await context.GetModule<FindRepoPaths>();
        var isRestored = await context.IsSuccessful(context.GetModuleIfRegistered<RestoreSolution>());

        return await context.DotNet().Build(
            options: new()
            {
                ProjectSolution = module.ValueOrDefault!.Solution,
                NoRestore = isRestored,
                Configuration = "Release",
            },
            null,
            cancellationToken: cancellationToken);
    }
}
