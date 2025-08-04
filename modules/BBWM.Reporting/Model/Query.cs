using BBWM.Core.Data;

namespace BBWM.Reporting.Model
{
    public class Query : IAuditableEntity
    {
        public int Id { get; set; }

        public Guid? DbDocFolderId { get; set; }

        public bool ForEndUserOnly { get; set; }


        public QueryFilterSet RootFilterSet { get; set; }


        public IList<QueryFilterSet> QueryFilterSets { get; set; } = new List<QueryFilterSet>();

        public IList<QueryTable> QueryTables { get; set; } = new List<QueryTable>();

        public IList<QueryTableJoin> QueryTableJoins { get; set; } = new List<QueryTableJoin>();
    }
}
