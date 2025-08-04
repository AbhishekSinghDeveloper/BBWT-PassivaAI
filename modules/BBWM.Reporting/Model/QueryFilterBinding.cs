using BBWM.Core.Data;
using BBWM.Reporting.Enums;

namespace BBWM.Reporting.Model
{
    public class QueryFilterBinding : IAuditableEntity
    {
        public int Id { get; set; }

        public QueryFilterBindingType BindingType { get; set; }


        public int QueryFilterId { get; set; }

        public QueryFilter QueryFilter { get; set; }

        public int? FilterControlId { get; set; }

        public FilterControl FilterControl { get; set; }

        public Guid? MasterDetailSectionId { get; set; }

        public Section MasterDetailSection { get; set; }

        public int? MasterDetailQueryTableColumnId { get; set; }

        public QueryTableColumn MasterDetailQueryTableColumn { get; set; }
    }
}
