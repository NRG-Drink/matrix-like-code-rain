using NRG.Matrix.Build.Modules.Common;
using NRG.Matrix.Build.Modules.Packing;
using NRG.Matrix.Build.Modules.Publishing;
using NRG.Matrix.Build.Modules.Releasing;
using NRG.Matrix.Build.Modules.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ModularPipelines;
using ModularPipelines.Extensions;
using Microsoft.Extensions.Hosting;

namespace NRG.Matrix.Build;

internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = Pipeline.CreateBuilder();

        builder.Configuration
            .AddJsonFile("appsettings.Production.json", optional: true)
            .AddEnvironmentVariables();

        builder.Services.Configure<NuGetSettings>(builder.Configuration.GetSection("NuGet"));
        builder.Services.Configure<NuGetStoreSettings>(builder.Configuration.GetSection("NuGetStore"));

        builder.ConfigureServices((context, collection) =>
        {
            collection.PostConfigure<FindRepoPathSettings>(s =>
            {
                s.ManipulateAfterFinish = repo =>
                {
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
            //.AddModule<FindRepoPaths>()
            .AddModule<GitVersion>()
            //.AddModule<BuildSolution>()
            //.AddModule<RunTests>()
            //.AddModule<PackNuGet>()
            // Server Only
            //.AddModule<UploadNuGetToStore>()
            //.AddModule<CreateReleaseGitHub>()
            ;

        var pipeline = await builder.BuildAsync();
        var summary = await pipeline.RunAsync();
    }
}
