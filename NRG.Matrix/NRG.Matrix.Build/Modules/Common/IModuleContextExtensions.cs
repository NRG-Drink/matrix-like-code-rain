using ModularPipelines.Context;
using ModularPipelines.Modules;
using System.Diagnostics.CodeAnalysis;

namespace NRG.Matrix.Build.Modules.Common;

public static class IModuleContextExtensions
{
    extension(IModuleContext context)
    {
        public bool TryGetModule<T>([NotNullWhen(true)] out T? module) where T : class, IModule
        {
            module = context.GetModuleIfRegistered<T>();
            return module is not null;
        }

        public async Task<bool> IsSuccessful<T>(Module<T>? module)
        {
            if (module is null)
            {
                return false;
            }

            var res = await module;
            return res.IsSuccess;
        }
    }
}
