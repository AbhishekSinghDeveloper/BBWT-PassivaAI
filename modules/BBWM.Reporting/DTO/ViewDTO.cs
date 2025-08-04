using BBWM.Core.DTO;

namespace BBWM.Reporting.DTO
{
    public class ViewDTO : IDTO
    {
        public int Id { get; set; }


        public Guid SectionId { get; set; }

        public SectionDTO Section { get; set; }


        public GridViewDTO GridView { get; set; }

        public IList<FilterControlDTO> Filters { get; set; }
    }
}
