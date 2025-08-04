using BBWM.Core.Membership.Model;

namespace BBWM.FormIO.Models
{
    public class FormDefinitionOrganization
    {
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public int FormDefinitionId { get; set; }
        public FormDefinition FormDefinition { get; set; } = null!;
    }
}