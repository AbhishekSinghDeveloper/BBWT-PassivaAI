using BBWM.Core.Data;
using BBWM.Reporting.Enums;

namespace BBWM.Reporting.Model
{
    public class GridView : IAuditableEntity
    {
        public int Id { get; set; }

        public SortOrder? DefaultSortOrder { get; set; }

        public bool ShowVisibleColumnsSelector { get; set; }

        public bool SummaryFooterVisible { get; set; }


        public int ViewId { get; set; }

        public View View { get; set; }

        public int? DefaultSortColumnId { get; set; }

        public QueryTableColumn DefaultSortColumn { get; set; }


        public IList<GridViewColumn> ViewColumns { get; set; } = new List<GridViewColumn>();
    }
}
