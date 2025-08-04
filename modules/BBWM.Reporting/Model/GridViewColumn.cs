using BBWM.Core.Data;
using BBWM.DbDoc.Model;

namespace BBWM.Reporting.Model
{
    public class GridViewColumn : IAuditableEntity
    {
        public int Id { get; set; }

        public int SortOrder { get; set; }

        public bool InheritHeader { get; set; }

        public string Header { get; set; }

        public bool Sortable { get; set; }

        public bool Visible { get; set; }

        public string ExtraSettings { get; set; }

        public string Footer { get; set; }


        public int GridViewId { get; set; }

        public GridView GridView { get; set; }

        public int QueryTableColumnId { get; set; }

        public QueryTableColumn QueryTableColumn { get; set; }

        public Guid? CustomColumnTypeId { get; set; }

        public ColumnType CustomColumnType { get; set; }
    }
}
