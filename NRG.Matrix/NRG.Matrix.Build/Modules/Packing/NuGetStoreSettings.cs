using ModularPipelines.Attributes;

namespace NRG.Matrix.Build.Modules.Packing;

public class NuGetStoreSettings
{
    [SecretValue]
    public string? ApiKey { get; init; }
    public string? Source { get; init; }
    public string? StoreName { get; init; }
    public bool SkipDuplicate { get; init; } = true;
}
