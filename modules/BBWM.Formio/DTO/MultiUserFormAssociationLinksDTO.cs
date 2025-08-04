using BBWM.Core.DTO;
using BBWM.Core.Membership.DTO;

namespace BBWM.FormIO.DTO
{
    /// <summary>
    /// This entity is the one that links a MUF definition with its Form Data for each Stage and allow a way to track which stage are completed or not.
    /// </summary>
    public class MultiUserFormAssociationLinksDTO : IDTO
    {
        public int Id { get; set; }
        public string? ExternalUserEmail { get; set; }
        public bool IsFilled { get; set; }
        public DateTime Completed { get; set; }
        public string SecurityCode { get; set; } = null!;

        // Foreign keys and navigational properties.
        public int MultiUserFormStageId { get; set; }
        public virtual MultiUserFormStageDTO MultiUserFormStage { get; set; } = null!;

        public string? UserId { get; set; }
        public virtual UserDTO? User { get; set; }
    }

    public class NewMultiUserFormAssociationLinksDTO
    {
        public string? UserId { get; set; }
        public string? ExternalUserEmail { get; set; }
        public int StageId { get; set; }
    }
}