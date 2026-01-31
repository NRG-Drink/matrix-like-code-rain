using EnumerableAsyncProcessor.Extensions;
using NRG.Matrix.Build.Modules.Common;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.Models;
using ModularPipelines.Modules;

namespace NRG.Matrix.Build.Modules.Testing;

[ModuleCategory(ModuleCategoryKey.Test)]
[DependsOn<FindRepoPaths>]
//[DependsOn<CleanTestOutputModule>(Optional = true)]
[DependsOn<RestoreSolution>(Optional = true)]
[DependsOn<BuildSolution>(Optional = true)]
[NotInParallel(NotInParallelKey.TestExecute)]
public class RunTests(IOptions<RunTestsSettings> testSettings) : Module<CommandResult[]>
{
    protected override async Task<CommandResult[]?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var repoPaths = await context.GetModule<FindRepoPaths>();
        var isRestored = await context.IsSuccessful(context.GetModuleIfRegistered<RestoreSolution>());
        var isBuilt = await context.IsSuccessful(context.GetModuleIfRegistered<BuildSolution>());

        var testReportArgs = new RunTestsOptions()
        {
            Coverage = true,
            CoverageOutputFormat = "cobertura",
            TreenodeFilter = "/*/*/*/*[Category!=NotRunInPipeline]",
            ResultsDirecotry = repoPaths.ValueOrDefault!.TestResultsData.Path,
            Arguments = [],
        };

        var processOptions = repoPaths.ValueOrDefault!.TestProjects
            .Select(e =>
            {
                var optionCopy = testReportArgs with { };
                testSettings.Value!.RunArgs(e, optionCopy);
                return (File: e, TestRunOptions: optionCopy);
            })
            .ToArray();

        var results = await processOptions
            .ToAsyncProcessorBuilder()
            .SelectAsync(e => context.SubModule(e.File.NameWithoutExtension, () => context.DotNet().Run(new()
            {
                Project = e.File.Path,
                NoRestore = isRestored,
                NoBuild = isBuilt,
                Configuration = "Release",
                Arguments = e.TestRunOptions.ToArguments(),
            })))
            .ProcessInParallel();

        return results;
    }
}
