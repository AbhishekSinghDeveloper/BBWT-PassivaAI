using BBWM.Core.DTO;

namespace BBWM.Reporting.DTO
{
    public class QueryTableDTO : IDTO
    {
        public int Id { get; set; }

        public string Alias { get; set; }

        public string SourceTableId { get; set; }

        public string SourceCode { get; set; }

        public string SelfJoinDbDocColumnId { get; set; }

        public bool OnlyForJoin { get; set; }


        public int QueryId { get; set; }

        public QueryDTO Query { get; set; }


        public IList<QueryTableColumnDTO> Columns { get; set; } = new List<QueryTableColumnDTO>();
    }
}
