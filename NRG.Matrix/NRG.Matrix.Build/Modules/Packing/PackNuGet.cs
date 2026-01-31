using EnumerableAsyncProcessor.Extensions;
using NRG.Matrix.Build.Modules.Common;
using NRG.Matrix.Build.Modules.Testing;
using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.DotNet.Extensions;
using ModularPipelines.DotNet.Options;
using ModularPipelines.FileSystem;
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
        var artifacts = repoPaths.ValueOrDefault!.Artifacts;
        artifacts.Create();

        var isRestored = await context.IsSuccessful(context.GetModuleIfRegistered<RestoreSolution>());
        var isBuilt = await context.IsSuccessful(context.GetModuleIfRegistered<BuildSolution>());

        var versionModule = await context.GetModule<GitVersion>();
        var packTemp = Path.Combine(artifacts.Path, $".nuget-pack_{DateTime.Now:yyyy-MM-dd_HHmmss}");
        Directory.CreateDirectory(packTemp);
        var options = new DotNetPackOptions()
        {
            //NoDependencies = true,
            NoRestore = isRestored || isBuilt,
            NoBuild = isBuilt,
            Configuration = "Release",
            //Output = artifacts.Path,
            Output = packTemp,
            Properties = [
                ("PackageVersion", versionModule.ValueOrDefault!),
                ("Version", versionModule.ValueOrDefault!),
            ],
        };

        var toPack = repoPaths.ValueOrDefault!.LibraryProjects
            .Concat(repoPaths.ValueOrDefault!.ToolProjects)
            .ToArray();

        var results = await toPack
            .ToAsyncProcessorBuilder()
            .SelectAsync(e =>
                context.SubModule(
                    e.NameWithoutExtension,
                    () => PackToTempFolder(context, options, e)
                )
            )
            .ProcessInParallel();

        var res = results.Select(e => GetOutputPath(e, artifacts.Path)).ToArray();
        Directory.Delete(packTemp, true);
        return res;
    }

    private PathCommandResult<MPFile> GetOutputPath(PathCommandResult<Folder> r, string outDir)
    {
        var pack = r.Path!.GetFiles("*.nupkg").FirstOrDefault();
        var packDestination = Path.Combine(outDir, pack!.Name);
        if (System.IO.File.Exists(packDestination))
        {
            System.IO.File.Delete(packDestination);
        }

        System.IO.File.Move(pack!.Path, packDestination);
        Directory.Delete(r.Path!.Path, true);

        return new(packDestination, r.CommandResult);
    }

    private async Task<PathCommandResult<Folder>> PackToTempFolder(IModuleContext context, DotNetPackOptions options, MPFile toPack)
    {
        var f = new Folder(Path.Combine(options.Output!, Guid.NewGuid().ToString()));
        var o = options with { ProjectSolution = toPack.Path, Output = f.Path };
        var result = await context.DotNet().Pack(o);

        return new(f, result);
    }
}
