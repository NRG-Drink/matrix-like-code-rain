using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.Models;
using ModularPipelines.Modules;

namespace NRG.Matrix.Build.Modules.Testing;

[ModuleCategory(ModuleCategoryKey.TestReport)]
[NotInParallel(nameof(ToolInstallReportGenerator))]
public class ToolInstallReportGenerator : Module<CommandResult>
{
    protected override async Task<CommandResult?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var result = await context.DotNet().Tool.Execute(
            new() { Tool = "dotnet-reportgenerator-globaltool", Arguments = ["--global"] },
            new(),
            cancellationToken);
        return result;
    }
}
