//using Kemartec.Checkliste.Build.Modules.Common;
//using Microsoft.Extensions.Logging;
//using ModularPipelines.Attributes;
//using ModularPipelines.Context;
//using ModularPipelines.Modules;

//namespace ISE.Pipeline.Modules.Common;

//[RunOnLocalMachineOnly]
//[DependsOn<FindRepoPaths>]
//[NotInParallel(nameof(CleanTestOutputModule))]
//public class CleanReleaseAssetsModule : Module<bool>
//{
//    protected override Task<bool> ShouldIgnoreFailures(IPipelineContext context, Exception exception)
//    {
//        context.Logger.LogError(exception, "Failed to clean/create");
//        return Task.FromResult(true);
//    }

//    protected override async Task<bool> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
//    {
//        var repoPaths = await GetModule<FindRepoPaths>();
//        repoPaths.Value!.ReleaseAssets.Create().Clean();

//        return true;
//    }
//}

//[RunOnLocalMachineOnly]
//[DependsOn<FindRepoPaths>]
//[NotInParallel(nameof(CleanTestOutputModule))]
//public class CleanTestOutputModule : Module<bool>
//{
//    protected override Task<bool> ShouldIgnoreFailures(IPipelineContext context, Exception exception)
//    {
//        context.Logger.LogError(exception, "Failed to clean/create");
//        return Task.FromResult(true);
//    }

//    protected override async Task<bool> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
//    {
//        var repoPaths = await GetModule<FindRepoPaths>();
//        repoPaths.Value!.TestResultsBase.Create().Clean();

//        return true;
//    }
//}

//[RunOnLocalMachineOnly]
//[DependsOn<FindRepoPaths>]
//[NotInParallel(nameof(CleanArtifactsModule))]
//public class CleanArtifactsModule : Module<bool>
//{
//    protected override Task<bool> ShouldIgnoreFailures(IPipelineContext context, Exception exception)
//    {
//        context.Logger.LogError(exception, "Failed to clean/create");
//        return Task.FromResult(true);
//    }

//    protected override async Task<bool> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
//    {
//        var repoPaths = await GetModule<FindRepoPaths>();
//        repoPaths.Value!.Artifacts.Create().Clean();

//        return true;
//    }
//}
