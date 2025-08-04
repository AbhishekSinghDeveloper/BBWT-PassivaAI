using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using System.Globalization;

namespace BBWM.Core.ModelHashing;

public static class FilterQueryStringValueProviderExtensions
{
    /// <summary>
    /// Provides fixed Query values where the "_original" suffix has removed.
    /// </summary>
    public static void AddOriginalFiltersFixingValueProvider(this MvcOptions options) =>
        options.ValueProviderFactories.Insert(0, new FilterQueryStringValueProviderFactory());
}

public class FilterQueryStringValueProvider : QueryStringValueProvider
{
    private readonly IQueryCollection _values;
    private readonly CultureInfo _culture;


    public FilterQueryStringValueProvider(BindingSource bindingSource, IQueryCollection values, CultureInfo culture) :
        base(bindingSource, values, culture)
    {
        _values = values;
        _culture = culture;
    }


    public override ValueProviderResult GetValue(string key)
    {
        if (key.StartsWith("Filters") && key.EndsWith("PropertyName"))
        {
            var propertyValue = (string)_values[key];
            if (propertyValue.EndsWith("_original"))
                return new ValueProviderResult(propertyValue.Remove(propertyValue.Length - 9), _culture);
        }

        return base.GetValue(key);
    }
}

public class FilterQueryStringValueProviderFactory : IValueProviderFactory
{
    public Task CreateValueProviderAsync(ValueProviderFactoryContext context)
    {
        if (context is null)
        {
            throw new ArgumentNullException(nameof(context));
        }

        var query = context.ActionContext.HttpContext.Request.Query;
        if (query is not null && query.Count > 0)
        {
            var filterStringValueProvider = new FilterQueryStringValueProvider(
                BindingSource.Query,
                query,
                CultureInfo.InvariantCulture);

            context.ValueProviders.Add(filterStringValueProvider);
        }

        return Task.CompletedTask;
    }
}
