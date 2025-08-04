using BBWM.Core.DTO;
using BBWM.Core.Membership.DTO;

namespace BBWM.FormIO.DTO
{
    public class FormRequestDTO : IDTO
    {
        public int Id { get; set; }
        public bool Completed { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public DateTimeOffset? CompletionDate { get; set; }

        // Foreign keys and navigational properties.
        public int FormDataId { get; set; }
        public virtual FormDataDTO? FormData { get; set; }

        public int FormRevisionId { get; set; }
        public virtual FormRevisionDTO? FormRevision { get; set; }

        public string RequesterId { get; set; } = null!;
        public UserDTO? Requester { get; set; }

        public List<string> GroupsIds { get; set; } = new();
        public List<string> UserIds { get; set; } = new();
    }

    public class FormRequestTargetsDTO
    {
        public List<FormRequestTargetGroupDTO> Groups { get; set; } = new();
        public List<FormRequestTargetUserDTO> Users { get; set; } = new();
    }

    public class FormRequestTargetGroupDTO
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    public class FormRequestTargetUserDTO
    {
        public string Id { get; set; } = null!;
        public string Name { get; set; } = null!;
    }

    public class FormRequestPageDTO : IDTO
    {
        public int Id { get; set; }
        public bool Completed { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public DateTimeOffset? CompletionDate { get; set; }

        // Foreign keys and navigational properties.
        public int FormDataId { get; set; }

        public int FormRevisionId { get; set; }
        public virtual FormRevisionForRequestMinDTO? FormRevision { get; set; }

        public string RequesterId { get; set; } = null!;
        public UserDTO? Requester { get; set; }

        public List<string> GroupsIds { get; set; } = new();
        public List<string> UserIds { get; set; } = new();
    }
}