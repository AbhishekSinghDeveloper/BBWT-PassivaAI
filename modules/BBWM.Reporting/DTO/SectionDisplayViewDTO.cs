using BBWM.DbDoc.DTO;
using BBWM.Reporting.Enums;
using System.Text.Json.Nodes;

namespace BBWM.Reporting.DTO
{
    public class SectionDisplayViewDTO
    {
        public string Title { get; set; }

        public string Description { get; set; }

        public SectionDataViewType DataViewType { get; set; }

        public ExpandBehaviour ExpandBehaviour { get; set; }

        public bool AutoCollapse { get; set; }

        public SortOrder DefaultSortOrder { get; set; }

        public string DefaultSortColumn { get; set; }

        public bool ShowVisibleColumnsSelector { get; set; }

        public bool SummaryFooterVisible { get; set; }

        public IList<SectionViewColumnDTO> Columns { get; set; }

        public IList<SectionViewFilterDTO> Filters { get; set; }

        /// <summary>
        /// A list of event types that a section being in master-section role, should emit to client-sections
        /// </summary>
        public IList<MasterSectionEmitEventType> MasterSectionEmittedEvents { get; set; }

        /// <summary>
        /// A list of bindings to master-section(s) used by a client-section to handle emitted event
        /// </summary>
        public IList<MasterSectionBindingDTO> MasterSectionBindings { get; set; }
    }

    public class SectionViewColumnDTO
    {
        public string TableAlias { get; set; }

        public int SortOrder { get; set; }

        public bool InheritHeader { get; set; }

        public string Header { get; set; }

        public bool Sortable { get; set; }

        public bool Visible { get; set; }

        public JsonNode ExtraSettings { get; set; }

        public ColumnMetadataDTO DbDocColumnMetadata { get; set; }

        public ColumnTypeDTO CustomColumnType { get; set; }

        public JsonNode Footer { get; set; }
    }

    public class SectionViewFilterDTO
    {
        public int FilterControlId { get; set; }

        public string Name { get; set; }

        public int SortOrder { get; set; }

        public string HintText { get; set; }

        public InputType InputType { get; set; }

        public FilterDataType? DataType { get; set; }

        public bool AutoSubmitInput { get; set; }

        public bool UserCanChangeOperator { get; set; }

        public JsonNode ExtraSettings { get; set; }

        public string DbDocColumnId { get; set; }

        public int? QueryFilterId { get; set; }

        public QueryRuleCode? QueryRuleCode { get; set; }
    }

    public class MasterSectionBindingDTO
    {
        public Guid? MasterSectionId { get; set; }

        public MasterSectionEmitEventType EventType { get; set; }

        /// <summary>
        /// A parameter that the master section passes to client section
        /// (e.g. in case of row selecting event, it passes column name applied as query filter to the dependent grid).
        /// </summary>
        public string FilterId { get; set; }

        public string ColumnId { get; set; }
    }
}
