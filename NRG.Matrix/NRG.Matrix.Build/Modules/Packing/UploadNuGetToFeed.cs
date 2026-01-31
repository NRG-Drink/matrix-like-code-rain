//using EnumerableAsyncProcessor.Extensions;
//using ISE.Pipeline;
//using Microsoft.Extensions.Options;
//using ModularPipelines.Attributes;
//using ModularPipelines.Context;
//using ModularPipelines.DotNet.Extensions;
//using ModularPipelines.DotNet.Options;
//using ModularPipelines.GitHub.Extensions;
//using ModularPipelines.Models;
//using ModularPipelines.Modules;
//using MPFile = ModularPipelines.FileSystem.File;

//namespace Kemartec.Checkliste.Build.Modules.Packing;

//[ModuleCategory(ModuleCategoryKey.UploadNuGet)]
//[RunOnServerOnly]
//[DependsOn<PackNuGet>]
//[NotInParallel(NotInParallelKey.UploadArtifacts)]
//public class UploadNuGetToFeed(IOptions<NuGetSettings> nugetSettings) : Module<CommandResult[]>
//{
//    private const string IseAgNuGetIndexJson = "https://nuget.pkg.github.com/iseag/index.json";

//    protected override async Task<SkipDecision> ShouldSkip(IPipelineContext context)
//    {
//        if (context.GitHub().EnvironmentVariables.Token is null)
//        {
//            return SkipDecision.Skip("GitHub.Token is null");
//        }

//        var packModule = await GetModule<PackNuGet>();
//        if (!packModule.Value!.Any())
//        {
//            return SkipDecision.Skip("No NuGets to upload");
//        }

//        return SkipDecision.DoNotSkip;
//    }

//    protected override async Task<CommandResult[]?> ExecuteAsync(IPipelineContext context, CancellationToken cancellationToken)
//    {
//        var packModule = await GetModule<PackNuGet>();
//        var nuGetPaths = packModule.Value!
//            .Select(e => e.Path)
//            .OfType<MPFile>()
//            .ToArray();

//        var apiKey = string.IsNullOrWhiteSpace(nugetSettings.Value.ApiKey) 
//            ? context.GitHub().EnvironmentVariables.Token
//            : nugetSettings.Value.ApiKey;
//        var source = string.IsNullOrWhiteSpace(nugetSettings.Value.Source)
//            ? IseAgNuGetIndexJson
//            : nugetSettings.Value.Source;

//        var options = new DotNetNugetPushOptions()
//        {
//            ApiKey = apiKey,
//            Source = source,
//            SkipDuplicate = true,
//        };

//        var results = await nuGetPaths
//            .ToAsyncProcessorBuilder()
//            .SelectAsync(e => SubModule(
//                e.NameWithoutExtension,
//                () => context.DotNet().Nuget.Push(options with { Path = e.Path }, cancellationToken)
//            ), cancellationToken)
//            .ProcessInParallel();

//        return results;
//    }

//    private static async Task<CommandResult> PushNuGet(
//        MPFile path,
//        IPipelineContext context,
//        DotNetNugetPushOptions options,
//        CancellationToken cancellationToken)
//    {
//        var result = await context.DotNet().Nuget.Push(options with { Path = path.Path }, cancellationToken);
//        //var message = $"NuGet Create for {path.NameWithoutExtension} -> {options.Source}";
//        //if (result.ExitCode is 0)
//        //{
//        //    context.LogOnPipelineEnd($"✅📤 Successfully {message}");
//        //}
//        //else
//        //{
//        //    context.LogOnPipelineEnd($"❌📤 Failed to {message}");
//        //}

//        return result;
//    }
//}
