using AutoMapper;
using BBWM.Core.Data;
using BBWM.FormIO.DTO;

namespace BBWM.FormIO.Models
{
    /// <summary>
    /// This entity is the one that links a MUF definition with its Form Data for each Stage and allow a way to track which stage are completed or not.
    /// </summary>
    public class MultiUserFormAssociations : IEntity
    {
        public int Id { get; set; }
        public DateTime Created { get; set; }
        public string Description { get; set; } = null!;

        /// <summary>
        /// Pointing to the current step on the sequence
        /// </summary>
        public int ActiveStepSequenceIndex { get; set; }

        public int TotalSequenceSteps { get; set; }

        // Foreign keys and navigational properties.
        public int FormDataId { get; set; }
        public FormData FormData { get; set; } = null!;

        public int MultiUserFormDefinitionId { get; set; }
        public MultiUserFormDefinition MultiUserFormDefinition { get; set; } = null!;

        public virtual ICollection<MultiUserFormAssociationLinks> MultiUserFormAssociationLinks { get; set; } = new List<MultiUserFormAssociationLinks>();

        public static void RegisterMap(IMapperConfigurationExpression c)
        {
            c.CreateMap<MultiUserFormAssociations, MultiUserFormAssociationsDTO>()
                .ForMember(multiUserFormAssociationsDto => multiUserFormAssociationsDto.FormDefinition, member => member
                    .MapFrom(multiUserFormAssociations => multiUserFormAssociations.FormData.FormDefinition))
                .ForMember(multiUserFormAssociationsDto => multiUserFormAssociationsDto.FormRevision, member => member
                    .MapFrom(multiUserFormAssociations => multiUserFormAssociations.FormData.FormDefinition!.FormRevisions
                        .FirstOrDefault(revision => revision.Id == multiUserFormAssociations.FormData.FormDefinition.ActiveRevisionId)))
                .ForMember(multiUserFormAssociationsDto => multiUserFormAssociationsDto.ActiveStageAssociation, member => member
                    .MapFrom(multiUserFormAssociations => multiUserFormAssociations.MultiUserFormAssociationLinks
                        .Where(multiUserFormAssociationLinks => multiUserFormAssociationLinks.MultiUserFormStage.SequenceStepIndex
                                                                == multiUserFormAssociations.ActiveStepSequenceIndex)
                        .Select(multiUserFormAssociationLinks => multiUserFormAssociationLinks.Id)))
                .ReverseMap();
        }
    }
}