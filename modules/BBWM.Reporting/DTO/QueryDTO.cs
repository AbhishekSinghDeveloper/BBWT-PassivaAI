using BBWM.Core.DTO;

namespace BBWM.Reporting.DTO
{
    public class QueryDTO : IDTO
    {
        public int Id { get; set; }

        public Guid? DbDocFolderId { get; set; }

        public bool ForEndUserOnly { get; set; }


        public QueryFilterSetDTO RootFilterSet { get; set; }


        public IList<QueryFilterSetDTO> QueryFilterSets { get; set; } = new List<QueryFilterSetDTO>();

        public IList<QueryTableDTO> QueryTables { get; set; } = new List<QueryTableDTO>();

        public IList<QueryTableJoinDTO> QueryTableJoins { get; set; } = new List<QueryTableJoinDTO>();
    }
}
