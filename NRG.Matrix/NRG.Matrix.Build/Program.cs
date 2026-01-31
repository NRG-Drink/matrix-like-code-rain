using NRG.Matrix.Build.Modules.Common;
using NRG.Matrix.Build.Modules.Packing;
using NRG.Matrix.Build.Modules.Publishing;
using NRG.Matrix.Build.Modules.Releasing;
using NRG.Matrix.Build.Modules.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularPipelines;
using ModularPipelines.Extensions;

namespace NRG.Matrix.Build;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = Pipeline.CreateBuilder();

        builder.Configuration
            .AddEnvironmentVariables();

        builder.Services.Configure<NuGetSettings>(builder.Configuration.GetSection("NuGet"));

        builder.ConfigureServices((context, collection) =>
        {
            collection.PostConfigure<FindRepoPathSettings>(s =>
            {
                s.ManipulateAfterFinish = repo =>
                {
                    repo.LibraryProjects.Clear();
                    repo.ExeProjects.RemoveAll(e => e.NameWithoutExtension.EndsWith("Build"));
                };
            });

            collection.PostConfigure<GitVersionSettings>(e => e.VersionFunc = e.VersionFunc);
        });

        builder.ConfigurePipelineOptions((context, options) =>
        {
            options.PrintLogo = false;
        });

        builder
            .AddModule<FindRepoPaths>()
            .AddModule<GitVersion>()
            .AddModule<BuildSolution>()
            .AddModule<RunTests>()
            .AddModule<Publish>()
            .AddModule<CreateReleaseGitHub>()
            .AddModule<UploadReleaseAssetsGitHub>()
            ;

        var pipeline = await builder.BuildAsync();
        var summary = await pipeline.RunAsync();
    }
}
