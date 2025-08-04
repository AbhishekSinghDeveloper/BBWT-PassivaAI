using BBWM.Core.Data;
using BBWM.Reporting.Enums;

namespace BBWM.Reporting.Model
{
    public class QueryRule : IAuditableEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public QueryRuleCode Code { get; set; }

        public string MySqlCodeTemplate { get; set; }

        public string MsSqlCodeTemplate { get; set; }


        public IList<QueryRuleType> RuleTypes { get; set; } = new List<QueryRuleType>();
    }
}
