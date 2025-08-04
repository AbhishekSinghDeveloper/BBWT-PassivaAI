using BBF.Reporting.Core.Interfaces;
using BBWM.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace BBF.Reporting.Web;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class NamedWidgetSourceAuthorize : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    // Implement service injection for this attribute.
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var widgetSourceService = serviceProvider.GetService<INamedWidgetSourceService>();
        if (widgetSourceService == null) throw new BusinessException("Cannot get widget source service.");

        return new NamedWidgetSourceAuthorizeAttribute(widgetSourceService);
    }

    // Attribute class.
    private class NamedWidgetSourceAuthorizeAttribute : IActionFilter
    {
        private readonly INamedWidgetSourceService _widgetSourceService;

        public NamedWidgetSourceAuthorizeAttribute(INamedWidgetSourceService widgetSourceService)
            => _widgetSourceService = widgetSourceService;

        // Capture the request before execution.
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;

            // If authorization requirement was disabled, return.
            if (request.Query.TryGetValue("disableAuthorization", out var disableAuthorization)
                && Convert.ToBoolean(disableAuthorization))
                return;

            // If there is widget source id in the request, and the widget source id is a valid guid value,
            // and the user has not access rights to the widget source corresponding to this id, return forbid result.
            if (request.RouteValues.TryGetValue("widgetSourceId", out var widgetSourceRawId)
                && widgetSourceRawId != null
                && Guid.TryParse(widgetSourceRawId.ToString(), out var widgetSourceId)
                && !_widgetSourceService.UserHasAccessToWidgetSource(widgetSourceId).Result)
            {
                context.Result = new ForbidResult();
            }
            // If there is widget code in the request,
            // and the user has not access rights to the widget corresponding to this url, return forbid result.
            else if (request.RouteValues.TryGetValue("widgetCode", out var widgetRawCode)
                     && widgetRawCode is string widgetCode
                     && !_widgetSourceService.UserHasAccessToWidgetSource(widgetCode).Result)
            {
                context.Result = new ForbidResult();
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}