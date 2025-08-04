using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Membership.Model;
using BBWM.FormIO.DTO;

namespace BBWM.FormIO.Models
{
    public class FormDefinition : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string? ViewName { get; set; }
        public bool? ByRequestOnly { get; set; } = true;

        // To track the active FormRevision.
        public int ActiveRevisionId { get; set; }

        // Foreign keys and navigational properties.
        public string? ManagerId { get; set; }
        public User? Manager { get; set; }

        public int? FormCategoryId { get; set; }
        public FormCategory? FormCategory { get; set; }

        public ICollection<FormData> FormData { get; set; } = new List<FormData>();
        public ICollection<FormRevision> FormRevisions { get; set; } = new List<FormRevision>();
        public ICollection<FormDefinitionOrganization> FormDefinitionOrganizations { get; set; } = new List<FormDefinitionOrganization>();

        public static void RegisterMap(IMapperConfigurationExpression expression)
        {
            expression.CreateMap<FormDefinition, FormDefinitionPageDTO>()
                .ForMember(formDefinitionPageDto => formDefinitionPageDto.IsPublished, member => member
                    .MapFrom(formDefinition => formDefinition.FormDefinitionOrganizations.Any()))
                .ForMember(formDefinitionPageDto => formDefinitionPageDto.OrganizationIds, member => member
                    .MapFrom(formDefinition => formDefinition.FormDefinitionOrganizations.Select(organization => organization.OrganizationId)))
                .ForMember(formDefinitionPageDto => formDefinitionPageDto.ActiveRevision, member => member
                    .MapFrom(formDefinition => formDefinition.FormRevisions.FirstOrDefault(revision => revision.Id == formDefinition.ActiveRevisionId)))
                .ForMember(formDefinitionPageDto => formDefinitionPageDto.Creator, member => member.Ignore())
                .ForMember(formDefinitionPageDto => formDefinitionPageDto.Org, member => member.Ignore())
                .ForMember(formDefinitionPageDto => formDefinitionPageDto.Category, member => member
                    .MapFrom(formDefinition => formDefinition.FormCategory != null ? formDefinition.FormCategory.Name : string.Empty))
                .ForMember(formDefinitionPageDto => formDefinitionPageDto.FormDataCount, member => member
                    // Form data json is empty when the user has a pending survey.
                    .MapFrom(formDefinition => formDefinition.FormData.Count(formData => formData.Json != string.Empty)))
                .ReverseMap();

            expression.CreateMap<FormDefinition, FormDefinitionDTO>()
                .ForMember(formDefinitionDto => formDefinitionDto.IsPublished, member => member
                    .MapFrom(formDefinition => formDefinition.FormDefinitionOrganizations.Any()))
                .ForMember(formDefinitionDto => formDefinitionDto.OrganizationIds, member => member
                    .MapFrom(formDefinition => formDefinition.FormDefinitionOrganizations.Select(organization => organization.OrganizationId)))
                .ForMember(formDefinitionDto => formDefinitionDto.ActiveRevision, member => member
                    .MapFrom(formDefinition => formDefinition.FormRevisions.FirstOrDefault(revision => revision.Id == formDefinition.ActiveRevisionId)))
                .ReverseMap();

            expression.CreateMap<FormDefinition, FormDefinitionComposedDTO>()
                .ForMember(formDefinitionComposedDto => formDefinitionComposedDto.IsPublished, member => member
                    .MapFrom(formDefinition => formDefinition.FormDefinitionOrganizations.Any()))
                .ForMember(formDefinitionComposedDto => formDefinitionComposedDto.OrganizationIds, member => member
                    .MapFrom(formDefinition => formDefinition.FormDefinitionOrganizations.Select(organization => organization.OrganizationId)))
                .ForMember(formDefinitionComposedDto => formDefinitionComposedDto.ActiveRevision, member => member
                    .MapFrom(formDefinition => formDefinition.FormRevisions.FirstOrDefault(revision => revision.Id == formDefinition.ActiveRevisionId)))
                .ForMember(formDefinitionComposedDto => formDefinitionComposedDto.Fields, member => member.Ignore())
                .ForMember(formDefinitionComposedDto => formDefinitionComposedDto.Json, member => member.Ignore())
                .ForMember(formDefinitionComposedDto => formDefinitionComposedDto.MobileFriendly, member => member.Ignore())
                .ReverseMap();

            expression.CreateMap<FormDefinition, FormDefinitionForNewRequestDTO>()
                .ForMember(formDefinitionForNewRequestDto => formDefinitionForNewRequestDto.FormRevisionData, member => member.Ignore())
                .ReverseMap();

            expression.CreateMap<FormDefinition, FormDefinitionForRequestMinDTO>().ReverseMap();

            expression.CreateMap<FormDefinition, FormDefinitionForFormDataPageDTO>().ReverseMap();
        }
    }
}