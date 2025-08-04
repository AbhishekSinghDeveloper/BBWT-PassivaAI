using BBWM.Core.Data;
using BBWM.Reporting.Enums;

namespace BBWM.Reporting.Model
{
    public class QueryRuleType : IAuditableEntity
    {
        public int Id { get; set; }

        public QueryRuleDataType Type { get; set; }


        public int QueryRuleId { get; set; }

        public QueryRule QueryRule { get; set; }
    }
}
