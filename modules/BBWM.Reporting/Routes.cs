using BBWM.Core.Web;

namespace BBWM.Reporting;

public static class Routes
{
    public static readonly Route Reports = new("/app/reporting/reports", "Reports");
    public static readonly Route ReportCreation = new("/app/reporting/reports/create", "Report Creation");
    public static readonly Route ReportEditing = new("/app/reporting/reports/edit/:reportId", "Report Editing");
    public static readonly Route NamedQueries = new("/app/reporting/named-queries", "Named Queries");
    public static readonly Route NamedQueryCreation = new("/app/reporting/named-queries/create", "Named Query Creation");
    public static readonly Route NamedQueryEditing = new("/app/reporting/named-queries/edit/:namedQueryId", "Named Query Editing");
}