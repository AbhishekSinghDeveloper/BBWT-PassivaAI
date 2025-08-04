using BBWM.Core.DTO;

namespace BBWM.FormIO.DTO
{
    public class MultiUserFormStagePermissionsDTO : IDTO
    {
        public int Id { get; set; }
        public string TabKey { get; set; } = null!;
        public byte Action { get; set; }

        // Foreign keys and navigational properties.
        public int MultiUserFormStageId { get; set; }
    }

    public class NewMultiUserFormPermissionDTO
    {
        public string TabKey { get; set; } = null!;
        public int StageId { get; set; }
        public byte Action { get; set; }
    }
}