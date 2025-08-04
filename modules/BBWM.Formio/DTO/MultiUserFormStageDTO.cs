using BBWM.Core.DTO;
using BBWM.Core.Membership.DTO;
using BBWM.FormIO.Enums;

namespace BBWM.FormIO.DTO
{
    public class MultiUserFormStageDTO : IDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public StageTargetType StageTargetType { get; set; }
        public string TabComponentKey { get; set; } = null!;
        public string InnerTabKey { get; set; } = null!;
        public bool ReviewerStage { get; set; }
        public int SequenceStepIndex { get; set; }

        // Foreign keys and navigational properties.
        public int MultiUserFormDefinitionId { get; set; }
        public MultiUserFormDefinitionDTO MultiUserFormDefinition { get; set; } = null!;

        public List<int> GroupIds { get; set; } = new();
        public List<GroupDTO> Groups { get; set; } = new();
        public List<MultiUserFormStagePermissionsDTO> MultiUserFormStagePermissions { get; set; } = new();
    }

    public class MultiUserFormStageUpdateDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public StageTargetType StageTargetType { get; set; }
        public bool ReviewerStage { get; set; }
        public int SequenceStepIndex { get; set; }

        // Foreign keys and navigational properties.
        public List<int> GroupIds { get; set; } = new();
    }
}