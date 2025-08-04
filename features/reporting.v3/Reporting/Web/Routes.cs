using BBWM.Core.Web;

namespace BBF.Reporting.Web;

public static class Routes
{
    #region These are temp. paths. Widgets should not be registered explicitely here - they will be registered in widget* projects

    public static readonly Route Queries = new("/app/reporting3/queries", "Named Queries");
    public static readonly Route QueryCreate = new("/app/reporting3/queries/create", "Create Query");
    public static readonly Route QueryEdit = new("/app/reporting3/queries/edit/:queryId", "Edit Query");
    public static readonly Route Widgets = new("/app/reporting3/widgets", "Named Widgets");
    public static readonly Route WidgetCreate = new("/app/reporting3/widgets/:widgetType/create", "Create Widget");
    public static readonly Route WidgetEdit = new("/app/reporting3/widgets/:widgetType/edit/:widgetSourceId", "Edit Widget");

    #endregion
}