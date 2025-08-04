using BBWM.Core.DTO;
using BBWM.Reporting.Enums;

namespace BBWM.Reporting.DTO
{
    public class SectionDTO : IDTO<Guid>
    {
        public Guid Id { get; set; }


        public string Title { get; set; }

        public bool ShowTitle { get; set; }

        public string Description { get; set; }

        public SectionDataViewType DataViewType { get; set; }

        public ExpandBehaviour ExpandBehaviour { get; set; }

        public bool AutoCollapse { get; set; }

        public bool Visible { get; set; }

        public int RowIndex { get; set; }

        public int ColumnIndex { get; set; }


        public Guid? PublishedSectionId { get; set; }

        public Guid ReportId { get; set; }

        public ReportDTO Report { get; set; }

        public Guid? NamedQueryId { get; set; }

        public NamedQueryDTO NamedQuery { get; set; }

        public Guid? ReusedSectionId { get; set; }

        public SectionDTO ReusedSection { get; set; }

        public int? QueryId { get; set; }

        public QueryDTO Query { get; set; }


        public ViewDTO View { get; set; }

        public IEnumerable<QueryFilterBindingDTO> QueryFilterBindings { get; set; } = new List<QueryFilterBindingDTO>();
    }
}
