//using ISE.Pipeline.Modules.Packing;
//using Kemartec.Checkliste.Build.Modules.Common;
//using ModularPipelines.Attributes;
//using ModularPipelines.Context;
//using ModularPipelines.Models;
//using ModularPipelines.Modules;

//namespace ISE.Pipeline.Modules.Common;

//[RunOnLocalMachineOnly]
//[RunOnWindows]
//[DependsOn<FindRepoPaths>]
//[DependsOn<PackNuGet>(IgnoreIfNotRegistered = true)]
//[NotInParallel(nameof(OpenArtifactsFolder))]
//public class OpenArtifactsFolder : Module<CommandResult>
//{
//    protected override Task<bool> ShouldIgnoreFailures(IPipelineContext context, Exception exception)
//        => Task.FromResult(true);

//    protected override async Task<CommandResult?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
//    {
//        var repoPaths = await GetModule<FindRepoPaths>();
//        var outDir = repoPaths.Value!.Artifacts;
//        return await context.Powershell.Script(new("explorer") { Arguments = [outDir.Path] }, cancellationToken);
//    }
//}
