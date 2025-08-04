using BBWM.Core.Data;

namespace BBWM.Reporting.Model
{
    public class NamedQuery : IAuditableEntity<Guid>
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public DateTime CreatedOn { get; set; }

        public DateTime UpdatedOn { get; set; }

        public bool IsDraft { get; set; }


        public string CreatedBy { get; set; }

        public string UpdatedBy { get; set; }

        public int QueryId { get; set; }

        public Query Query { get; set; }
    }
}
