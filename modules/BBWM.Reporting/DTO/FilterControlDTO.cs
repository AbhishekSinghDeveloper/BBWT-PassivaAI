using BBWM.Core.DTO;
using BBWM.Reporting.Enums;
using System.Text.Json.Nodes;

namespace BBWM.Reporting.DTO
{
    public class FilterControlDTO : IDTO
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int SortOrder { get; set; }

        public string HintText { get; set; }

        public InputType InputType { get; set; }

        public FilterDataType? DataType { get; set; }

        public bool AutoSubmitInput { get; set; }

        public bool UserCanChangeOperator { get; set; }

        public JsonNode ExtraSettings { get; set; }


        public int ViewId { get; set; }

        public ViewDTO View { get; set; }

        public int? MasterControlId { get; set; }

        public FilterControlDTO MasterControl { get; set; }


        public IEnumerable<QueryFilterBindingDTO> QueryFilterBindings { get; set; }
    }
}
