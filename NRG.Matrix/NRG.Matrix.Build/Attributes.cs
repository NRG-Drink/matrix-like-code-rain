using ModularPipelines.Attributes;
using ModularPipelines.Context;
using ModularPipelines.GitHub.Extensions;

namespace NRG.Matrix.Build;

public class RunOnServerOnlyAttribute : MandatoryRunConditionAttribute
{
    public override async Task<bool> Condition(IPipelineHookContext context)
        => context.GitHub().EnvironmentVariables.CI;
}

public class RunOnLocalMachineOnlyAttribute : MandatoryRunConditionAttribute
{
    public override async Task<bool> Condition(IPipelineHookContext context)
        => !context.GitHub().EnvironmentVariables.CI;
}
