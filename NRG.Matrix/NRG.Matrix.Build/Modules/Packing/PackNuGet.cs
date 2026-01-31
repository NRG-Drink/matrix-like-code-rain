using EnumerableAsyncProcessor.Extensions;
using NRG.Matrix.Build.Modules.Common;
using NRG.Matrix.Build.Modules.Testing;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.Models;
using ModularPipelines.Modules;
using MPFile = ModularPipelines.FileSystem.File;

namespace NRG.Matrix.Build.Modules.Packing;

[ModuleCategory(ModuleCategoryKey.Pack)]
[DependsOn<FindRepoPaths>]
[DependsOn<GitVersion>]
[DependsOn<RestoreSolution>(Optional = true)]
[DependsOn<BuildSolution>(Optional = true)]
[DependsOn<RunTests>(Optional = true)]
//[DependsOn<CleanArtifactsModule>(Optional = true)]
[NotInParallel(NotInParallelKey.BuildExecute, NotInParallelKey.PackExecute)]
public class PackNuGet : Module<PathCommandResult<MPFile>[]>
{
    protected override async Task<PathCommandResult<MPFile>[]?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var repoPaths = await context.GetModule<FindRepoPaths>();
        var outDir = repoPaths.ValueOrDefault!.Artifacts;
        outDir.Create();

        var isRestored = await context.IsSuccessful(context.GetModuleIfRegistered<RestoreSolution>());
        var isBuilt = await context.IsSuccessful(context.GetModuleIfRegistered<BuildSolution>());

        var versionModule = await context.GetModule<GitVersion>();
        var options = new DotNetPackOptions()
        {
            //NoDependencies = true,
            NoRestore = isRestored || isBuilt,
            NoBuild = isBuilt,
            Configuration = "Release",
            Output= outDir.Path,
            Properties = [
                ("PackageVersion", versionModule.ValueOrDefault!),
                ("Version", versionModule.ValueOrDefault!),
            ],
        };

        var results = await repoPaths.ValueOrDefault!.LibraryProjects
            .ToAsyncProcessorBuilder()
            .SelectAsync(e =>
                context.SubModule(
                    e.NameWithoutExtension, 
                    () => context.DotNet().Pack(options with { ProjectSolution = e.Path }, 
                    cancellationToken: cancellationToken)
                )
            )
            .ProcessInParallel();

        var start = "Successfully created package '";
        var end = "'.";
        return [.. results.Select(e => ExtractNuGetPath(e, start, end))];
    }

    private static PathCommandResult<MPFile> ExtractNuGetPath(CommandResult e, string start, string end)
    {
        if (!e.StandardOutput.Contains(start) || !e.StandardOutput.Contains(end))
        {
            return new PathCommandResult<MPFile>(null, e);
        }

        var stout = e.StandardOutput.Trim();
        var index = stout.IndexOf(start);
        var filePath = stout[(index + start.Length)..^(end.Length)];
        var result = new PathCommandResult<MPFile>(new(filePath), e);
        return result;
    }

    private static async Task<CommandResult> PackANuGet(
        MPFile path,
        IPipelineContext context,
        DotNetPackOptions options,
        CancellationToken cancellationToken)
    {
        var result = await context.DotNet().Pack(options with { ProjectSolution = path.Path }, cancellationToken: cancellationToken);
        //var message = $"NuGet Packed for {path.NameWithoutExtension}";
        //if (result.ExitCode is 0)
        //{
        //    context.LogOnPipelineEnd($"✅📦 Successfully {message}");
        //}
        //else
        //{
        //    context.LogOnPipelineEnd($"❌📦 Failed to {message}");
        //}

        return result;
    }
}
