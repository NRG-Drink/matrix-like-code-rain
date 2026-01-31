using ModularPipelines.Attributes;

namespace NRG.Matrix.Build.Modules.Packing;

public class NuGetSettings
{
    [SecretValue]
    public string? ApiKey { get; init; }
    public string? Source { get; init; }
}
