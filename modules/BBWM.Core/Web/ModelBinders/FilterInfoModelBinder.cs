using BBWM.Core.Filters;
using BBWM.Core.Filters.TypedFilters;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using System.Reflection;
using System.Web;

namespace BBWM.Core.Web.ModelBinders;

public class FilterInfoModelBinder : IModelBinder
{
    private readonly IModelMetadataProvider _metadataProvider;
    private readonly Dictionary<string, IModelBinder> _binders;

    public FilterInfoModelBinder(IModelMetadataProvider metadataProvider, Dictionary<string, IModelBinder> binders)
    {
        _metadataProvider = metadataProvider;
        _binders = binders;
    }

    public async Task BindModelAsync(ModelBindingContext bindingContext)
    {
        if (bindingContext is null)
        {
            throw new ArgumentNullException(nameof(bindingContext));
        }

        if (bindingContext.ModelType == typeof(FilterInfoBase))
        {
            var type = bindingContext.ValueProvider.GetValue($"{bindingContext.ModelName?.ToLowerInvariant()}.$type");
            if (type == ValueProviderResult.None)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            var filterType = Assembly.GetAssembly(typeof(FilterInfoBase)).GetTypes()
                .FirstOrDefault(a => a.GetTypeInfo().IsSubclassOf(typeof(FilterInfoBase)) &&
                    !a.IsAbstract &&
                    string.Equals(a.Name, $"{type.FirstValue}Filter", StringComparison.InvariantCultureIgnoreCase));

            if (filterType is null)
            {
                bindingContext.Result = ModelBindingResult.Failed();
                return;
            }

            var binder = _binders[filterType.FullName];
            var metadata = _metadataProvider.GetMetadataForType(filterType);

            ModelBindingResult result;
            using (bindingContext.EnterNestedScope(metadata, bindingContext.FieldName, bindingContext.ModelName, null))
            {
                await binder.BindModelAsync(bindingContext);
                result = bindingContext.Result;

                var propertyNameProperty = result.Model.GetType().GetProperty("PropertyName");
                if (propertyNameProperty is not null)
                {
                    var propertyName = propertyNameProperty.GetValue(result.Model) as string;
                    if (!string.IsNullOrEmpty(propertyName))
                        propertyNameProperty.SetValue(result.Model, HttpUtility.UrlDecode(propertyName));
                }

                if (filterType == typeof(StringFilter))
                {
                    var valueProperty = result.Model.GetType().GetProperty("Value");
                    if (valueProperty is not null)
                    {
                        var value = valueProperty.GetValue(result.Model) as string;
                        if (!string.IsNullOrEmpty(value))
                            valueProperty.SetValue(result.Model, HttpUtility.UrlDecode(value));
                    }
                }
            }

            bindingContext.Result = result;
        }
    }
}
