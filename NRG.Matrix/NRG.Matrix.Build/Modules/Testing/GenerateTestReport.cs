using NRG.Matrix.Build.Modules.Common;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Configuration;
using ModularPipelines.Context;
using ModularPipelines.FileSystem;
using ModularPipelines.Models;
using ModularPipelines.Modules;
using ModularPipelines.Options;

namespace NRG.Matrix.Build.Modules.Testing;

[ModuleCategory(ModuleCategoryKey.TestReport)]
[DependsOn<FindRepoPaths>]
[DependsOn<ToolInstallReportGenerator>]
[DependsOn<RunTests>(Optional = true)]
[NotInParallel(NotInParallelKey.TestExecute)]
public class GenerateTestReport(IOptions<RunTestsSettings> reportSettings) : Module<PathCommandResult<Folder>>
{
    protected override ModuleConfiguration Configure()
        => ModuleConfiguration.Create()
            .WithSkipWhen(async (context) =>
            {
                var repoPaths = await context.GetModule<FindRepoPaths>();

                if (!repoPaths.ValueOrDefault!.TestResultsRun.Exists)
                {
                    return SkipDecision.Skip("No test run folder found");
                }

                return SkipDecision.DoNotSkip;
            })
            .Build();

    protected override async Task<PathCommandResult<Folder>?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var repoPaths = await context.GetModule<FindRepoPaths>();

        var arguments = new List<string>
        {
            "-reports:**/*.cobertura.xml",
            $"-targetdir:{repoPaths.ValueOrDefault!.TestResultsReport.Path}",
            "-reporttypes:Html,MarkdownSummaryGithub",
            "-assemblyfilters:-*Test*",
            $"-historydir:\"{repoPaths.ValueOrDefault!.TestHistory}\""
        };
        reportSettings.Value!.ReportArgs(arguments);

        // TODO: Make arguments configurable
        var result = await context.Shell.Command.ExecuteCommandLineTool(
            new GenericCommandLineToolOptions("reportgenerator") { Arguments = arguments },
            new() { WorkingDirectory = repoPaths.ValueOrDefault!.TestResultsRun },
            cancellationToken);

        //context.LogOnPipelineEnd($"✅📤 Successfully Test Report Generate");

        return new(repoPaths.ValueOrDefault!.TestResultsReport, result);
    }
}
