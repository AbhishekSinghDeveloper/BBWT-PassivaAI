using BBWM.Core.DTO;

namespace BBWM.Reporting.DTO
{
    public class QueryTableColumnDTO : IDTO
    {
        public int Id { get; set; }

        public string SourceColumnId { get; set; }

        public bool OnlyForJoin { get; set; }


        public int QueryTableId { get; set; }

        public QueryTableDTO QueryTable { get; set; }
    }
}
