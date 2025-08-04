using BBWM.Core.Data;

namespace BBWM.Reporting.Model
{
    public class QueryFilter : IAuditableEntity
    {
        public int Id { get; set; }

        public string CustomSqlCodeTemplate { get; set; }

        public object Value { get; set; }

        public object Value2 { get; set; }


        public int QueryFilterSetId { get; set; }

        public QueryFilterSet QueryFilterSet { get; set; }

        public int? QueryTableColumnId { get; set; }

        public QueryTableColumn QueryTableColumn { get; set; }

        public int? QueryRuleId { get; set; }

        public QueryRule QueryRule { get; set; }


        public IEnumerable<QueryFilterBinding> QueryFilterBindings { get; set; } = new List<QueryFilterBinding>();
    }
}
