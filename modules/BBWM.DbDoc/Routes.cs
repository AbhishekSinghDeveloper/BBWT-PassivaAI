using BBWM.Core.Web;

namespace BBWM.DbDoc;

public static class Routes
{
    public static readonly Route DbExplorer = new("/app/dbdoc/db-explorer", "Database Explorer");
    public static readonly Route ColumnTypes = new("/app/dbdoc/column-types", "Database Column Types");
    public static readonly Route AddColumnType = new("/app/dbdoc/column-types/add", "Add Custom Column Type");
    public static readonly Route EditColumnType = new("/app/dbdoc/column-types/edit/:id", "Edit Custom Column Type");
}
