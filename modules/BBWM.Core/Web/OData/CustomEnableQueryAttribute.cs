using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OData.Query;

using System.Linq.Expressions;

namespace BBWM.Core.Web.OData;

/// <summary>
/// Base class that provides the ability to customize OData requests handling.
/// </summary>
public abstract class CustomEnableQueryAttribute : EnableQueryAttribute
{
    private string _originalUrl;
    private ODataQueryOptions _modifiedODataQueryOptions;
    private IDictionary<string, Expression<Func<IQueryable, string, ODataQueryString, IQueryable>>> _customizations;

    public sealed override void OnActionExecuting(ActionExecutingContext actionExecutingContext)
    {
        OnActionExecuting(actionExecutingContext,
            new ODataQueryString(actionExecutingContext.HttpContext.Request.QueryString.Value.Replace("%20", " ")));
        base.OnActionExecuting(actionExecutingContext);
    }

    public sealed override void ValidateQuery(HttpRequest request, ODataQueryOptions queryOptions)
    {
        CustomValidate(request, queryOptions);
        InitCustomization(queryOptions);
        base.ValidateQuery(request, _modifiedODataQueryOptions);
    }

    public sealed override IQueryable ApplyQuery(IQueryable queryable, ODataQueryOptions queryOptions)
    {
        var oDataQueryStringOriginal = new ODataQueryString(_originalUrl.Replace("%20", " "));
        foreach (var customization in _customizations)
        {
            queryable = customization.Value.Compile()(queryable, customization.Key, oDataQueryStringOriginal);
        }

        return base.ApplyQuery(queryable, _modifiedODataQueryOptions);
    }

    /// <summary>
    /// Returns a dictionary of modifications that will be applied to the resulting <see cref="IQueryable"/>.
    /// </summary>
    public abstract IDictionary<string, Expression<Func<IQueryable, string, ODataQueryString, IQueryable>>>
        GetCustomizations();

    /// <summary>
    /// Use this method to provide extra check of the OData query and the context.
    /// </summary>
    ///<remarks>Here you may check User's access to specified OData query based on his claims.</remarks>
    public virtual void OnActionExecuting(ActionExecutingContext context, ODataQueryString queryOptions) { }

    /// <summary>
    /// Use this method for extra validation of OData query.
    /// </summary>
    public virtual void CustomValidate(HttpRequest request, ODataQueryOptions queryOptions) { }


    private void InitCustomization(ODataQueryOptions queryOptions)
    {
        _customizations = GetCustomizations(); // Getting customization rules
        _originalUrl = queryOptions.Request.QueryString.Value; // Saving original request to not lose custom fields in the URL

        // Removing custom filters from URL
        var oDataQueryString = new ODataQueryString(queryOptions.Request.QueryString.Value.Replace("%20", " "));
        foreach (var key in _customizations.Keys)
        {
            if (oDataQueryString.ContainsFilter(key))
                oDataQueryString.RemoveFilter(key);
        }

        // Removing custom ordering from URL
        var orderFieldName = oDataQueryString.GetOrderFieldName();
        if (!string.IsNullOrWhiteSpace(orderFieldName) && _customizations.Keys.Any(x => orderFieldName.Equals(x, StringComparison.InvariantCultureIgnoreCase)))
            oDataQueryString.RemoveOrdering();

        // Creation of the new options to avoid exceptions from OData native logic about custom fields
        queryOptions.Request.QueryString = new QueryString(oDataQueryString.Value.Replace(" ", "%20"));
        _modifiedODataQueryOptions = new ODataQueryOptions(queryOptions.Context, queryOptions.Request);
    }
}
