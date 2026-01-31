using EnumerableAsyncProcessor.Extensions;
using NRG.Matrix.Build.Modules.Common;
using NRG.Matrix.Build.Modules.Packing;
using NRG.Matrix.Build.Modules.Publishing;
using NRG.Matrix.Build.Modules.Testing;
using ModularPipelines.Attributes;
using ModularPipelines.Configuration;
using ModularPipelines.Context;
using ModularPipelines.FileSystem;
using ModularPipelines.GitHub.Extensions;
using ModularPipelines.Models;
using ModularPipelines.Modules;
using MPFile = ModularPipelines.FileSystem.File;
using Octokit;

namespace NRG.Matrix.Build.Modules.Releasing;

// TODO: Config upload NuGet, Exe, TestReport
// TODO: What if zipping or uploading a file fails? (It should proceed)
[ModuleCategory(ModuleCategoryKey.UploadReleaseAssets)]
[RunOnServerOnly]
[DependsOn<FindRepoPaths>]
[DependsOn<CreateReleaseGitHub>]
[DependsOn<GenerateTestReport>(Optional = true)]
[DependsOn<PackNuGet>(Optional = true)]
[DependsOn<Publish>(Optional = true)]
[NotInParallel(NotInParallelKey.UploadArtifacts)]
public class UploadReleaseAssetsGitHub : Module<ReleaseAsset[]>
{
    protected override ModuleConfiguration Configure()
        => ModuleConfiguration.Create()
            .WithSkipWhen(context =>
            {
                var report = context.GetModuleIfRegistered<GenerateTestReport>();
                var pack = context.GetModuleIfRegistered<PackNuGet>();
                var publish = context.GetModuleIfRegistered<Publish>();

                if (report is not null
                    || pack is not null
                    || publish is not null)
                {
                    return SkipDecision.DoNotSkip;
                }

                return SkipDecision.Skip("No Test, NuGet or Exe to upload.");
            })
            .Build();

    protected override async Task<ReleaseAsset[]?> ExecuteAsync(IModuleContext context, CancellationToken cancellationToken)
    {
        var repoPaths = await context.GetModule<FindRepoPaths>();

        var report = context.GetModuleIfRegistered<GenerateTestReport>();
        var pack = context.GetModuleIfRegistered<PackNuGet>();
        var publish = context.GetModuleIfRegistered<Publish>();

        // Zip NuGet and Exe
        var (zipFiles, zipFolders) = await GetFilesAndFoldersFromModules(report, pack, publish);
        if (repoPaths.ValueOrDefault!.ReleaseAssets.Exists)
        {
            var files = repoPaths.ValueOrDefault!.ReleaseAssets.GetFiles("*.*").ToArray();
            zipFiles = [.. zipFiles, .. files];
            var folders = repoPaths.ValueOrDefault!.ReleaseAssets.GetFolders(e => true).ToArray();
            zipFolders = [.. folders];
        }

        var releaseModule = await context.GetModule<CreateReleaseGitHub>();
        var release = releaseModule.ValueOrDefault!;
        var fileResults = await zipFiles
            .ToAsyncProcessorBuilder()
            .SelectAsync(e => context.SubModule(e.Name, async () =>
            {
                using var stream = await Utils.ZipFilesAndFolders([e], [], context.Logger);
                stream.Position = 0;
                var data = new ReleaseAssetUpload()
                {
                    FileName = e.Name,
                    ContentType = "application/zip",
                    RawData = stream,
                };
                return await context.GitHub().Client.Repository.Release.UploadAsset(release, data, cancellationToken);
            }))
            .ProcessInParallel();

        var folderResults = await zipFolders
            .ToAsyncProcessorBuilder()
            .SelectAsync(e => context.SubModule(e.Name, async () =>
            {
                using var stream = await Utils.ZipFilesAndFolders([], [e], context.Logger);
                stream.Position = 0;
                var data = new ReleaseAssetUpload()
                {
                    FileName = $"{e.Name}.zip",
                    ContentType = "application/zip",
                    RawData = stream,
                };
                return await context.GitHub().Client.Repository.Release.UploadAsset(release, data, cancellationToken);
            }))
            .ProcessInParallel();

        //return folderResults;
        return [.. fileResults, .. folderResults];

        //using var zipStream = await SubModule(
        //    "Create Zip",
        //    () => Utils.ZipFilesAndFolders(zipFiles, zipFolders, context.Logger)
        //);

        //context.Logger.LogInformation("Produced a zip with length {ZipLenght}", zipStream.Length);

        //// TODO: Upload assets as singles (Files as files, Folders as zip)
        //var result = await SubModule("Upload Assets", async () =>
        //{
        //    zipStream.Position = 0;
        //    var assetUpload = new ReleaseAssetUpload()
        //    {
        //        FileName = "assets.zip",
        //        ContentType = "application/zip",
        //        RawData = zipStream,
        //    };

        //    var releaseModul = await GetModule<CreateReleaseGitHub>();
        //    var assetResults = await context.GitHub().Client.Repository.Release.UploadAsset(releaseModul.Value!, assetUpload, cancellationToken);
        //    return assetResults;
        //});
        //context.LogOnPipelineEnd($"✅📤 Successfully Release Asset Upload {result.Name}");

        //return result;
    }

    private static async Task<(MPFile[] zipFiles, Folder[] zipFolders)> GetFilesAndFoldersFromModules(
        GenerateTestReport? report,
        PackNuGet? pack,
        Publish? publish)
    {
        var zipFiles = new List<MPFile>();
        var zipFolders = new List<Folder>();

        if (report is not null)
        {
            var r = await report;
            if (r.ValueOrDefault?.Path is not null)
            {
                zipFolders = [.. zipFolders, r.ValueOrDefault.Path];
            }
        }

        if (pack is not null)
        {
            var p = await pack;
            zipFiles = [.. zipFiles, .. p.ValueOrDefault!.Select(e => e.Path).OfType<MPFile>().ToArray()];
        }

        if (publish is not null)
        {
            var pub = await publish;
            zipFolders = [.. zipFolders, .. pub.ValueOrDefault!.Select(e => e.Path).OfType<Folder>().ToArray()];
        }

        return ([.. zipFiles], [.. zipFolders]);
    }
}
