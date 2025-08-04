using BBWM.Core.Data;

namespace BBWM.Reporting.Model
{
    public class View : IAuditableEntity
    {
        public int Id { get; set; }


        public Guid SectionId { get; set; }

        public Section Section { get; set; }


        public GridView GridView { get; set; }

        public IList<FilterControl> Filters { get; set; }
    }
}
