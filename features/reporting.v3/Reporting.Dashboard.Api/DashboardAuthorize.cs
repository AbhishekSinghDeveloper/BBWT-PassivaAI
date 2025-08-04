using BBF.Reporting.Dashboard.Interfaces;
using BBWM.Core.Exceptions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;

namespace BBF.Reporting.Dashboard.Api;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class DashboardAuthorize : Attribute, IFilterFactory
{
    public bool IsReusable => false;

    // Implement service injection for this attribute.
    public IFilterMetadata CreateInstance(IServiceProvider serviceProvider)
    {
        var dashboardService = serviceProvider.GetService<IDashboardService>();
        if (dashboardService == null) throw new BusinessException("Cannot get dashboard service.");

        return new DashboardAuthorizeAttribute(dashboardService);
    }

    // Attribute class.
    private class DashboardAuthorizeAttribute : IActionFilter
    {
        private readonly IDashboardService _dashboardService;

        public DashboardAuthorizeAttribute(IDashboardService dashboardService)
            => _dashboardService = dashboardService;

        // Capture the request before execution.
        public void OnActionExecuting(ActionExecutingContext context)
        {
            var request = context.HttpContext.Request;

            // If there is dashboard id in the request, and the dashboard id is a valid guid value,
            // and the user has not access to the dashboard corresponding to this id, return forbid result.
            if (request.RouteValues.TryGetValue("dashboardId", out var dashboardRawId)
                && dashboardRawId != null
                && Guid.TryParse(dashboardRawId.ToString(), out var dashboardId)
                && !_dashboardService.UserHasAccessToDashboard(dashboardId).Result)
            {
                context.Result = new ForbidResult();
            }
            // If there is dashboard code in the request,
            // and the user has not access to the dashboard corresponding to this url, return forbid result.
            else if (request.RouteValues.TryGetValue("dashboardCode", out var dashboardRawCode)
                     && dashboardRawCode is string dashboardCode
                     && !_dashboardService.UserHasAccessToDashboard(dashboardCode).Result)
            {
                context.Result = new ForbidResult();
            }
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
        }
    }
}