using BBWM.Core.Data;
using BBWM.Reporting.Enums;

namespace BBWM.Reporting.Model
{
    public class FilterControl : IAuditableEntity
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public int SortOrder { get; set; }

        public string HintText { get; set; }

        public InputType InputType { get; set; }

        public FilterDataType? DataType { get; set; }

        public bool AutoSubmitInput { get; set; }

        public bool UserCanChangeOperator { get; set; }

        public string ExtraSettings { get; set; }


        public int ViewId { get; set; }

        public View View { get; set; }

        public int? MasterControlId { get; set; }

        public FilterControl MasterControl { get; set; }


        public IList<QueryFilterBinding> QueryFilterBindings { get; set; }
    }
}
