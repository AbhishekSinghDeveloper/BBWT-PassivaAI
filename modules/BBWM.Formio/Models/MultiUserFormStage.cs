using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Membership.Model;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Enums;

namespace BBWM.FormIO.Models
{
    public class MultiUserFormStage : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public bool ReviewerStage { get; set; }
        public string InnerTabKey { get; set; } = null!;
        public StageTargetType StageTargetType { get; set; } = 0;

        /// <summary>
        /// The apikey of the Special Tab this stage is linked to
        /// </summary>
        public string TabComponentKey { get; set; } = null!;

        /// <summary>
        /// This is used for sequencing the list of stages in steps
        /// </summary>
        public int SequenceStepIndex { get; set; }

        // Foreign keys and navigational properties.
        public int MultiUserFormDefinitionId { get; set; }
        public MultiUserFormDefinition MultiUserFormDefinition { get; set; } = null!;

        public List<Group> Groups { get; set; } = new();

        public virtual ICollection<MultiUserFormStagePermissions> MultiUserFormStagePermissions { get; set; }
            = new List<MultiUserFormStagePermissions>();

        public static void RegisterMap(IMapperConfigurationExpression expression)
        {
            expression.CreateMap<MultiUserFormStage, MultiUserFormStageDTO>()
                .ForMember(multiUserFormStageDto => multiUserFormStageDto.GroupIds, member => member
                    .MapFrom(multiUserFormStage => multiUserFormStage.Groups.Select(group => group.Id)))
                .ReverseMap();
        }
    }
}