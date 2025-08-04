using BBWM.Core.DTO;

namespace BBWM.Reporting.DTO
{
    public class NamedQueryDTO : IDTO<Guid>
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public bool IsDraft { get; set; }


        public string CreatedById { get; set; }

        public string UpdatedById { get; set; }

        public int QueryId { get; set; }

        public QueryDTO Query { get; set; }
    }
}
