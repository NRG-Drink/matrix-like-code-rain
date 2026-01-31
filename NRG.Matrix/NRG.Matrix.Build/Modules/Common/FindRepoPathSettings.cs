using NRG.Matrix.Build;
using MPFile = ModularPipelines.FileSystem.File;

namespace NRG.Matrix.Build.Modules.Common;

public class FindRepoPathSettings
{
    public Func<IEnumerable<MPFile>, IEnumerable<MPFile>> ManipulaeAfterProjectDiscovery { get; set; } = e => e;
    public Action<RepoPaths> ManipulateAfterFinish { get; set; } = e => { };
}
