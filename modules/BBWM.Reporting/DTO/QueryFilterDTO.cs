using BBWM.Core.DTO;

namespace BBWM.Reporting.DTO
{
    public class QueryFilterDTO : IDTO
    {
        public int Id { get; set; }

        public object Value { get; set; }

        public object Value2 { get; set; }


        public string CustomSqlCodeTemplate { get; set; }
        public IEnumerable<SqlFilterCodeInsertDTO> CustomSqlCodeInserts { get; set; }


        public int QueryFilterSetId { get; set; }

        public QueryFilterSetDTO QueryFilterSet { get; set; }

        public int? QueryTableColumnId { get; set; }

        public QueryTableColumnDTO QueryTableColumn { get; set; }

        public int? QueryRuleId { get; set; }

        public QueryRuleDTO QueryRule { get; set; }


        public IEnumerable<QueryFilterBindingDTO> QueryFilterBindings { get; set; } = new List<QueryFilterBindingDTO>();
    }
}
