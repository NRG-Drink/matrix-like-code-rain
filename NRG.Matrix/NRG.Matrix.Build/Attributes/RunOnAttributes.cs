using System;

namespace ModularPipelines.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class RunOnServerOnlyAttribute : Attribute
{
}

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public sealed class RunOnLocalMachineOnlyAttribute : Attribute
{
}
