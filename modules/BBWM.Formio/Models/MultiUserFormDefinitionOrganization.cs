using BBWM.Core.Membership.Model;

namespace BBWM.FormIO.Models
{
    public class MultiUserFormDefinitionOrganization
    {
        public int OrganizationId { get; set; }
        public Organization Organization { get; set; } = null!;

        public int MultiUserFormDefinitionId { get; set; }
        public MultiUserFormDefinition MultiUserFormDefinition { get; set; } = null!;
    }
}