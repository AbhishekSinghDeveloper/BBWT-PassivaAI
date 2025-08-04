using System.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BBF.Reporting.Core.ModelBinders;

public class UrlDecodedModelBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        var valueProviderResult = bindingContext.ValueProvider.GetValue(bindingContext.ModelName);

        if (valueProviderResult != ValueProviderResult.None)
        {
            var value = valueProviderResult.FirstValue;
            if (!string.IsNullOrEmpty(value))
            {
                var decodedValue = HttpUtility.UrlDecode(value);
                bindingContext.Result = ModelBindingResult.Success(decodedValue);
                return Task.CompletedTask;
            }
        }

        bindingContext.Result = ModelBindingResult.Failed();
        return Task.CompletedTask;
    }
}