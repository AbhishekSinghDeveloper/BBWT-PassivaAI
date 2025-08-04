using BBWM.Core.DTO;
using BBWM.Reporting.Enums;

namespace BBWM.Reporting.DTO
{
    public class GridViewDTO : IDTO
    {
        public int Id { get; set; }

        public SortOrder? DefaultSortOrder { get; set; }

        public bool ShowVisibleColumnsSelector { get; set; }

        public bool SummaryFooterVisible { get; set; }


        public int ViewId { get; set; }

        public ViewDTO View { get; set; }

        public int? DefaultSortColumnId { get; set; }

        public QueryTableColumnDTO DefaultSortColumn { get; set; }


        public IList<GridViewColumnDTO> ViewColumns { get; set; } = new List<GridViewColumnDTO>();
    }
}
