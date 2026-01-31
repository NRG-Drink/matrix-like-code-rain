using EnumerableAsyncProcessor.Extensions;
using NRG.Matrix.Build.Modules.Common;
using NRG.Matrix.Build.Modules.Packing;
using NRG.Matrix.Build.Modules.Testing;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.FileSystem;
using ModularPipelines.Models;
using ModularPipelines.Modules;
using MPFile = ModularPipelines.FileSystem.File;

namespace NRG.Matrix.Build.Modules.Publishing;

[ModuleCategory(ModuleCategoryKey.Publish)]
[DependsOn<FindRepoPaths>]
[DependsOn<GitVersion>]
[DependsOn<BuildSolution>(Optional = true)]
[DependsOn<RunTests>(Optional = true)]
//[DependsOn<CleanArtifactsModule>(IgnoreIfNotRegistered = true)]
[DependsOn<PackNuGet>(Optional = true)]
[NotInParallel(NotInParallelKey.BuildExecute, NotInParallelKey.PublishExecute)]
public class Publish : Module<PathCommandResult<Folder>[]>
{
    protected override async Task<PathCommandResult<Folder>[]?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var repoPaths = await context.GetModule<FindRepoPaths>();
        var versionModule = await context.GetModule<GitVersion>();
        var outDir = repoPaths.ValueOrDefault!.Artifacts;
        outDir.Create();

        // TODO: Make options configurable (at least for runtime)
        // TODO: What about publishing a project for multiple runtimes?
        var options = new DotNetPublishOptions()
        {
            // Cannot build/restore solution for a specific runtime.
            Configuration = "Release",
            NoSelfContained = false,
            Runtime = "win-x64",
            Properties = [
                ("PackageVersion", versionModule.ValueOrDefault!),
                ("Version", versionModule.ValueOrDefault!),
            ],
        };

        var outDirs = repoPaths.ValueOrDefault!.ExeProjects.Concat(repoPaths.ValueOrDefault!.WinExeProjects)
            .Select(e => (File: e, OutDir: Path.Combine(outDir.Path, $"{e.NameWithoutExtension}_v{versionModule.ValueOrDefault!}")))
            .ToArray();

        var results = await outDirs
            .ToAsyncProcessorBuilder()
            .SelectAsync(e => context.SubModule(
                e.File.NameWithoutExtension,
                () => context.DotNet().Publish(options with { ProjectSolution = e.File.Path, Output= e.OutDir }, cancellationToken: cancellationToken)
            ))
            .ProcessOneAtATime();

        return results
            .Select(e =>
            {
                var outDir = outDirs.FirstOrDefault(o => e.CommandInput.Contains(o.OutDir)).OutDir ?? "";
                var result = new PathCommandResult<Folder>(new(outDir), e);
                return result;
            })
            .ToArray();
    }
}
