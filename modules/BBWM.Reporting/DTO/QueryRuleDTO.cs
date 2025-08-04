using BBWM.Core.DTO;
using BBWM.Reporting.Enums;

namespace BBWM.Reporting.DTO
{
    public class QueryRuleDTO : IDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public QueryRuleCode Code { get; set; }

        public string MySqlCodeTemplate { get; set; }

        public string MsSqlCodeTemplate { get; set; }


        public IList<QueryRuleTypeDTO> RuleTypes { get; set; } = new List<QueryRuleTypeDTO>();
    }
}
