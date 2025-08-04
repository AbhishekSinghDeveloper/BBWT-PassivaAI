using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Membership.Model;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Enums;

namespace BBWM.FormIO.Models
{
    public class MultiUserFormDefinition : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public int CurrentStage { get; set; }

        // Foreign keys and navigational properties.
        public string? CreatorId { get; set; }
        public User? Creator { get; set; }

        public int? FormRevisionId { get; set; }
        public FormRevision? FormRevision { get; set; }

        public ICollection<MultiUserFormStage> MultiUserFormStages { get; set; } = new List<MultiUserFormStage>();

        public ICollection<MultiUserFormAssociations> MultiUserFormAssociations { get; set; }
            = new List<MultiUserFormAssociations>();

        public ICollection<MultiUserFormDefinitionOrganization> MultiUserFormDefinitionOrganizations { get; set; }
            = new List<MultiUserFormDefinitionOrganization>();

        public static void RegisterMap(IMapperConfigurationExpression expression)
        {
            expression.CreateMap<MultiUserFormDefinition, MultiUserFormDefinitionDTO>()
                .ForMember(multiUserFormDefinitionDto => multiUserFormDefinitionDto.IsPublished, member => member
                    .MapFrom(multiUserFormDefinition => multiUserFormDefinition.MultiUserFormDefinitionOrganizations.Any()))
                .ForMember(multiUserFormDefinitionDto => multiUserFormDefinitionDto.SetupReady, member => member
                    .MapFrom(multiUserFormDefinition => !multiUserFormDefinition.MultiUserFormStages
                        .Any(stage => !stage.Groups.Any() && stage.StageTargetType == StageTargetType.InnerGroups)))
                .ForMember(multiUserFormDefinitionDto => multiUserFormDefinitionDto.OrganizationIds, member => member
                    .MapFrom(multiUserFormDefinition => multiUserFormDefinition.MultiUserFormDefinitionOrganizations
                        .Select(organization => organization.OrganizationId)))
                .ReverseMap();

            expression.CreateMap<MultiUserFormDefinition, MultiUserFormDefinitionAllDataDTO>()
                .ForMember(multiUserFormDefinitionAllDataDto => multiUserFormDefinitionAllDataDto.IsPublished, member => member
                    .MapFrom(multiUserFormDefinition => multiUserFormDefinition.MultiUserFormDefinitionOrganizations.Any()))
                .ForMember(multiUserFormDefinitionAllDataDto => multiUserFormDefinitionAllDataDto.OrganizationIds, member => member
                    .MapFrom(multiUserFormDefinition => multiUserFormDefinition.MultiUserFormDefinitionOrganizations
                        .Select(organization => organization.OrganizationId)))
                .ForMember(multiUserFormDefinitionAllDataDto => multiUserFormDefinitionAllDataDto.SetupReady, member => member
                    .MapFrom(multiUserFormDefinition => !multiUserFormDefinition.MultiUserFormStages
                        .Any(stage => !stage.Groups.Any() && stage.StageTargetType == StageTargetType.InnerGroups)))
                .ReverseMap();
        }
    }
}