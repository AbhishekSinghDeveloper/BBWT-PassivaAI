using BBWM.Core.Data;
using BBWM.Reporting.Enums;

namespace BBWM.Reporting.Model
{
    public class Section : IAuditableEntity<Guid>
    {
        public Guid Id { get; set; }


        public string Title { get; set; }

        public bool ShowTitle { get; set; }

        /// <summary>
        /// Long text field, because it contains images of section's WYSIWYG editor.
        /// </summary>
        public string Description { get; set; }

        public SectionDataViewType DataViewType { get; set; }

        public ExpandBehaviour ExpandBehaviour { get; set; }

        public bool AutoCollapse { get; set; }

        public bool Visible { get; set; }

        public int RowIndex { get; set; }

        public int ColumnIndex { get; set; }


        public Guid? PublishedSectionId { get; set; }

        public Guid ReportId { get; set; }

        public Report Report { get; set; }

        public Guid? NamedQueryId { get; set; }

        public NamedQuery NamedQuery { get; set; }

        public Guid? ReusedSectionId { get; set; }

        public Section ReusedSection { get; set; }

        public int? QueryId { get; set; }

        public Query Query { get; set; }


        public View View { get; set; }

        public IEnumerable<QueryFilterBinding> QueryFilterBindings { get; set; } = new List<QueryFilterBinding>();
    }
}
