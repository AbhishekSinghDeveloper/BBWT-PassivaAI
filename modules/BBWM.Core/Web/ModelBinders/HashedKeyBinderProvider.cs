using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace BBWM.Core.Web.ModelBinders;

public class HashedKeyBinderProvider : IModelBinderProvider
{
    public IModelBinder GetBinder(ModelBinderProviderContext context) =>
        context.Metadata.ModelType == typeof(int) ? new BinderTypeModelBinder(typeof(HashedKeyBinder)) : null;
}
