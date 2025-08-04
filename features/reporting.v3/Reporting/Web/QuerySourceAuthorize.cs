using BBF.Reporting.Core.Interfaces;
using BBWM.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace BBF.Reporting.Web;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class NamedQuerySourceAuthorize : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    // Implement service injection for this attribute.
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var querySourceService = serviceProvider.GetService<INamedQuerySourceService>();
        if (querySourceService == null) throw new BusinessException("Cannot get query source service.");

        return new NamedQuerySourceAuthorizeAttribute(querySourceService);
    }

    // Attribute class.
    private class NamedQuerySourceAuthorizeAttribute : IActionFilter
    {
        private readonly INamedQuerySourceService _querySourceService;

        public NamedQuerySourceAuthorizeAttribute(INamedQuerySourceService querySourceService)
            => _querySourceService = querySourceService;

        // Capture the request before execution.
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;

            // If authorization requirement was disabled, return.
            if (request.Query.TryGetValue("disableAuthorization", out var disableAuthorization)
                && Convert.ToBoolean(disableAuthorization))
                return;

            // If there is query source id in the request, and the query source id is a valid guid value,
            // and the user has not access to the query source corresponding to this id, return forbid result.
            if (request.RouteValues.TryGetValue("querySourceId", out var querySourceRawId)
                && querySourceRawId != null
                && Guid.TryParse(querySourceRawId.ToString(), out var querySourceId)
                && !_querySourceService.UserHasAccessToQuerySource(querySourceId).Result)
            {
                context.Result = new ForbidResult();
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}