using EnumerableAsyncProcessor.Extensions;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Configuration;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Models;
using ModularPipelines.Modules;
using MPFile = ModularPipelines.FileSystem.File;

namespace NRG.Matrix.Build.Modules.Packing;

[ModuleCategory(ModuleCategoryKey.UploadNuGetStore)]
[RunOnServerOnly]
[DependsOn<PackNuGet>]
[NotInParallel(NotInParallelKey.UploadArtifacts)]
public class UploadNuGetToStore(IOptions<NuGetStoreSettings> settings) : Module<CommandResult[]>
{
    protected override ModuleConfiguration Configure()
        => ModuleConfiguration.Create()
            .WithSkipWhen(async (context) =>
            {
                if (string.IsNullOrWhiteSpace(settings.Value.ApiKey))
                {
                    return SkipDecision.Skip("NuGetStore.ApiKey is null or empty");
                }

                if (string.IsNullOrWhiteSpace(settings.Value.Source))
                {
                    return SkipDecision.Skip("NuGetStore.Source is null or empty");
                }

                var packModule = await context.GetModule<PackNuGet>();
                if (!packModule.ValueOrDefault!.Any())
                {
                    return SkipDecision.Skip("No NuGet packages to upload");
                }

                return SkipDecision.DoNotSkip;
            })
            .Build();

    protected override async Task<CommandResult[]?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var packModule = await context.GetModule<PackNuGet>();
        var nuGetPaths = packModule.ValueOrDefault!
            .Select(e => e.Path)
            .OfType<MPFile>()
            .ToArray();

        var options = new DotNetNugetPushOptions()
        {
            ApiKey = settings.Value.ApiKey,
            Source = settings.Value.Source,
            SkipDuplicate = settings.Value.SkipDuplicate,
        };

        var results = await nuGetPaths
            .ToAsyncProcessorBuilder()
            .SelectAsync(e => context.SubModule(
                e.NameWithoutExtension,
                () => context.DotNet().Nuget.Push(options with { Path = e.Path }, cancellationToken: cancellationToken)
            ), cancellationToken)
            .ProcessInParallel();

        return results;
    }
}
