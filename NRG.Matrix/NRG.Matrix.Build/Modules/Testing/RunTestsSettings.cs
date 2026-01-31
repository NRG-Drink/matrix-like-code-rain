using MPFile = ModularPipelines.FileSystem.File;

namespace NRG.Matrix.Build.Modules.Testing;

public class RunTestsSettings
{
    public Action<MPFile, RunTestsOptions> RunArgs { get; set; } = (projectName, options) => { };
    public Action<List<string>> ReportArgs { get; set; } = (options) => { };
}
