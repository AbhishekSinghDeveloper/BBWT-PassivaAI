using BBWM.Core.Web;

namespace BBF.Reporting.Dashboard.Api;

public static class Routes
{
    public static readonly Route Dashboards = new("/app/reporting3/dashboards", "Dashboards");
    public static readonly Route DashboardCreate = new("/app/reporting3/dashboard/create", "Create Dashboard");
    public static readonly Route DashboardEdit = new("/app/reporting3/dashboard/edit/:dashboardId", "Edit Dashboard");
}