using BBWM.Core.DTO;
using BBWM.FormIO.Models;

namespace BBWM.FormIO.DTO
{
    /// <summary>
    /// This entity is the one that links a MUF definition with its Form Data for each Stage and allow a way to track which stage are completed or not.
    /// </summary>
    public class MultiUserFormAssociationsDTO : IDTO
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public string Description { get; set; } = null!;
        public int TotalSequenceSteps { get; set; }
        public int ActiveStepSequenceIndex { get; set; }

        // Foreign keys and navigational properties.
        public int FormDataId { get; set; }
        public virtual FormData FormData { get; set; } = null!;

        public int MultiUserFormDefinitionId { get; set; }
        public virtual MultiUserFormDefinition MultiUserFormDefinition { get; set; } = null!;

        public List<int> ActiveStageAssociation { get; set; } = new();

        public virtual FormRevisionDTO FormRevision { get; set; } = null!;
        public virtual FormDefinitionDTO FormDefinition { get; set; } = null!;

        public virtual ICollection<MultiUserFormAssociationLinks> MultiUserFormAssociationLinks { get; set; }
            = new List<MultiUserFormAssociationLinks>();
    }

    public class NewMultiUserFormAssociationsDTO
    {
        public string Description { get; set; } = null!;
        public DateTime Created { get; set; }

        // Foreign keys and navigational properties.
        public int MultiUserFormDefinitionId { get; set; }
        public List<NewMultiUserFormAssociationLinksDTO> MultiUserFormAssociationLinks { get; set; } = new();
    }
}