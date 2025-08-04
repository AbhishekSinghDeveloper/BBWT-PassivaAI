using BBWM.Core.DTO;
using BBWM.Reporting.Enums;

namespace BBWM.Reporting.DTO
{
    public class QueryFilterSetDTO : IDTO
    {
        public int Id { get; set; }

        public QueryConditionalOperator ConditionalOperator { get; set; }


        public int QueryId { get; set; }

        public QueryDTO Query { get; set; }

        public int? ParentId { get; set; }

        public QueryFilterSetDTO Parent { get; set; }

        public int? ParentQueryId { get; set; }

        public QueryDTO ParentQuery { get; set; }


        public IList<QueryFilterSetDTO> ChildSets { get; set; } = new List<QueryFilterSetDTO>();

        public IList<QueryFilterDTO> QueryFilters { get; set; } = new List<QueryFilterDTO>();
    }
}
