using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BBWM.Core.ModuleLinker;

public interface IConfigureMvcModuleLinkage
{
    IEnumerable<IModelBinderProvider> GetModelBinderProviders();
}
