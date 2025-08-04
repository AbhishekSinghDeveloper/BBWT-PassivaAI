using BBWM.Core.DTO;
using BBWM.Core.Membership.DTO;

namespace BBWM.FormIO.DTO
{
    public class MultiUserFormDefinitionDTO : IDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int CurrentStage { get; set; }
        public bool SetupReady { get; set; }
        public bool IsPublished { get; set; }

        // Foreign keys and navigational properties.
        public string? CreatorId { get; set; }
        public UserDTO? Creator { get; set; }

        public virtual List<MultiUserFormStageDTO> MultiUserFormStages { get; set; } = new();

        public int? FormRevisionId { get; set; }
        public FormRevisionDTO? FormRevision { get; set; }

        public List<int> OrganizationIds { get; set; } = new();
    }

    public class MultiUserFormDefinitionAllDataDTO : MultiUserFormDefinitionDTO
    {
    }

    public class TabObject
    {
        /// <summary>
        /// APIKey of the state-Tab component
        /// </summary>
        public string TabComponent { get; set; } = null!;

        /// <summary>
        /// Key of the inner tab
        /// </summary>
        public string InnerTab { get; set; } = null!;
    }

    public class NewMultiUserFormDefinitionDTO
    {
        public string Name { get; set; } = null!;
        public string CreatorId { get; set; } = null!;

        // Foreign keys and navigational properties.
        public int FormDefinitionId { get; set; }
        public List<TabObject> Tabs { get; set; } = new();
    }
}