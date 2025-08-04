using BBWM.Core.Data;
using BBWM.Reporting.Enums;

namespace BBWM.Reporting.Model
{
    public class QueryFilterSet : IAuditableEntity
    {
        public int Id { get; set; }

        public QueryConditionalOperator ConditionalOperator { get; set; }


        public int QueryId { get; set; }

        public Query Query { get; set; }

        public int? ParentId { get; set; }

        public QueryFilterSet Parent { get; set; }

        public int? ParentQueryId { get; set; }

        public Query ParentQuery { get; set; }


        public IList<QueryFilterSet> ChildSets { get; set; } = new List<QueryFilterSet>();

        public IList<QueryFilter> QueryFilters { get; set; } = new List<QueryFilter>();
    }
}
