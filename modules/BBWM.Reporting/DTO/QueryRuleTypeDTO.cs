using BBWM.Core.DTO;
using BBWM.Reporting.Enums;

namespace BBWM.Reporting.DTO
{
    public class QueryRuleTypeDTO : IDTO
    {
        public int Id { get; set; }

        public QueryRuleDataType Type { get; set; }


        public int QueryRuleId { get; set; }

        public QueryRuleDTO QueryRule { get; set; }
    }
}
