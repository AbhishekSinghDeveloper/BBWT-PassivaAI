using BBWM.Core.DTO;
using BBWM.Reporting.Enums;

namespace BBWM.Reporting.DTO
{
    public class QueryFilterBindingDTO : IDTO
    {
        public int Id { get; set; }

        public QueryFilterBindingType BindingType { get; set; }


        public int QueryFilterId { get; set; }

        public QueryFilterDTO QueryFilter { get; set; }

        public int? FilterControlId { get; set; }

        public FilterControlDTO FilterControl { get; set; }

        public Guid? MasterDetailSectionId { get; set; }

        public SectionDTO MasterDetailSection { get; set; }

        public int? MasterDetailQueryTableColumnId { get; set; }

        public QueryTableColumnDTO MasterDetailQueryTableColumn { get; set; }
    }
}
