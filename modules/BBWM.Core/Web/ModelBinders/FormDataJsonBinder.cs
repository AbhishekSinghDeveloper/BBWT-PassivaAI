using Microsoft.AspNetCore.Mvc.ModelBinding;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace BBWM.Core.Web.ModelBinders;

public class FormDataJsonBinder : IModelBinder
{
    public Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext is null) throw new ArgumentNullException(nameof(bindingContext));

        // Fetch the value of the argument by name and set it to the model state
        string fieldName = bindingContext.FieldName;
        var valueProviderResult = bindingContext.ValueProvider.GetValue(fieldName);
        if (valueProviderResult == ValueProviderResult.None) return Task.CompletedTask;
        else bindingContext.ModelState.SetModelValue(fieldName, valueProviderResult);

        // Do nothing if the value is null or empty
        string value = valueProviderResult.FirstValue;
        if (string.IsNullOrEmpty(value)) return Task.CompletedTask;

        try
        {
            // Deserialize the provided value and set the binding result
            object result = JsonSerializer.Deserialize(value, bindingContext.ModelType != null ? bindingContext.ModelType : typeof(JsonNode));
            bindingContext.Result = ModelBindingResult.Success(result);
        }
        catch (JsonException)
        {
            bindingContext.Result = ModelBindingResult.Failed();
        }

        return Task.CompletedTask;
    }
}