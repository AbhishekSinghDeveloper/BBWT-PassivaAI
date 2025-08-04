using System.Reflection;
using System.Web;
using BBF.Reporting.Core.Model.Variables;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace BBF.Reporting.Core.ModelBinders;

public class EmittedVariableModelBinder : IModelBinder
{
    private readonly IModelMetadataProvider _metadataProvider;
    private readonly Dictionary<string, IModelBinder> _binders;

    public EmittedVariableModelBinder(IModelMetadataProvider metadataProvider, Dictionary<string, IModelBinder> binders)
    {
        _metadataProvider = metadataProvider;
        _binders = binders;
    }

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext is null)
            throw new ArgumentNullException(nameof(bindingContext));

        if (bindingContext.ModelType == typeof(EmittedVariable))
        {
            var provider = bindingContext.ValueProvider.GetValue($"{bindingContext.ModelName.ToLowerInvariant()}.$type");
            if (provider == ValueProviderResult.None)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            var variableType = Assembly.GetAssembly(typeof(EmittedVariable))?.GetTypes().FirstOrDefault(type =>
                type.GetTypeInfo().IsSubclassOf(typeof(EmittedVariable)) && !type.IsAbstract &&
                string.Equals(type.Name, $"Emitted{provider.FirstValue}Variable", StringComparison.InvariantCultureIgnoreCase));

            if (variableType?.FullName == null)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            var binder = _binders[variableType.FullName];
            var metadata = _metadataProvider.GetMetadataForType(variableType);

            ModelBindingResult result;

            using (bindingContext.EnterNestedScope(metadata, bindingContext.FieldName, bindingContext.ModelName, null))
            {
                await binder.BindModelAsync(bindingContext);
                result = bindingContext.Result;

                if (result.Model != null)
                {
                    var nameProperty = result.Model.GetType().GetProperty("Name");
                    if (nameProperty is not null)
                    {
                        var propertyName = nameProperty.GetValue(result.Model) as string;
                        if (!string.IsNullOrEmpty(propertyName))
                            nameProperty.SetValue(result.Model, HttpUtility.UrlDecode(propertyName));
                    }

                    if (variableType == typeof(EmittedStringVariable))
                    {
                        var valueProperty = result.Model.GetType().GetProperty("Value");
                        if (valueProperty?.GetValue(result.Model) is string value)
                        {
                            valueProperty.SetValue(result.Model, HttpUtility.UrlDecode(value));
                        }
                    }

                    if (variableType == typeof(EmittedStringArrayVariable))
                    {
                        var valueProperty = result.Model.GetType().GetProperty("Value");
                        if (valueProperty?.GetValue(result.Model) is IEnumerable<string> values)
                        {
                            valueProperty.SetValue(result.Model, values.Select(HttpUtility.UrlDecode));
                        }
                    }
                }
            }

            bindingContext.Result = result;
        }
    }
}