using EnumerableAsyncProcessor.Extensions;
using Microsoft.Extensions.Options;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.GitHub.Extensions;
using ModularPipelines.Models;
using ModularPipelines.Modules;
using MPFile = ModularPipelines.FileSystem.File;

namespace NRG.Matrix.Build.Modules.Packing;

[ModuleCategory(ModuleCategoryKey.UploadNuGet)]
[RunOnServerOnly]
[DependsOn<PackNuGet>]
[NotInParallel(NotInParallelKey.UploadArtifacts)]
public class UploadNuGetToFeed(IOptions<NuGetSettings> nugetSettings) : Module<CommandResult[]>
{
    protected override async Task<CommandResult[]?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        if (context.GitHub().EnvironmentVariables.Token is null)
        {
            return Array.Empty<CommandResult>();
        }

        var packModule = await context.GetModule<PackNuGet>();
        PathCommandResult<MPFile>[]? packed = packModule.ValueOrDefault;
        if (packed is null || packed.Length == 0)
        {
            return Array.Empty<CommandResult>();
        }

        var nuGetPaths = packed
            .Select(e => e.Path)
            .OfType<MPFile>()
            .ToArray();

        var apiKey = string.IsNullOrWhiteSpace(nugetSettings.Value.ApiKey)
            ? context.GitHub().EnvironmentVariables.Token
            : nugetSettings.Value.ApiKey;
        var source = string.IsNullOrWhiteSpace(nugetSettings.Value.Source)
            ? throw new Exception("NuGet.Source is null or empty")
            : nugetSettings.Value.Source;

        var options = new DotNetNugetPushOptions()
        {
            ApiKey = apiKey,
            Source = source,
            SkipDuplicate = true,
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
