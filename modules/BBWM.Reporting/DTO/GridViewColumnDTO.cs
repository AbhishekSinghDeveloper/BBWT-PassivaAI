using BBWM.Core.DTO;
using BBWM.DbDoc.DTO;
using System.Text.Json.Nodes;

namespace BBWM.Reporting.DTO
{
    public class GridViewColumnDTO : IDTO
    {
        public int Id { get; set; }

        public int SortOrder { get; set; }

        public bool InheritHeader { get; set; }

        public string Header { get; set; }

        public bool Sortable { get; set; }

        public bool Visible { get; set; }

        public JsonNode ExtraSettings { get; set; }

        public JsonNode Footer { get; set; }


        public int GridViewId { get; set; }

        public GridViewDTO GridView { get; set; }

        public int QueryTableColumnId { get; set; }

        public QueryTableColumnDTO QueryTableColumn { get; set; }

        public Guid? CustomColumnTypeId { get; set; }

        public ColumnTypeDTO CustomColumnType { get; set; }
    }
}
