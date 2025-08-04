using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Membership.Model;
using BBWM.Core.DTO;
using BBWM.FormIO.DTO;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBWM.FormIO.Models
{
    public class FormData : IEntity
    {
        public int Id { get; set; }
        public string Json { get; set; } = null!;
        public DateTimeOffset CreatedOn { get; set; }

        // Foreign keys and navigational properties.
        public int? SurveyId { get; set; }
        public FormSurvey? Survey { get; set; }

        public int? OrganizationId { get; set; }
        public Organization? Organization { get; set; }

        public int? FormDefinitionId { get; set; }
        public FormDefinition? FormDefinition { get; set; }

        public string UserId { get; set; } = null!;
        [ForeignKey("UserId")] public User CreatedBy { get; set; } = null!;


        public static void RegisterMap(IMapperConfigurationExpression expression)
        {
            expression.CreateMap<FormData, FormDataDTO>()
                .ForMember(formDataDto => formDataDto.DraftId, member => member.Ignore())
                .ForMember(formDataDto => formDataDto.IsMUF, member => member.Ignore())
                .ForMember(formDataDto => formDataDto.MultiUserFormAssocLinkId, member => member.Ignore())
                .ForMember(formDataDto => formDataDto.FormDefinitionId, member => member.Ignore())
                .ForMember(formDataDto => formDataDto.RequestId, member => member.Ignore())
                .ForMember(formDataDto => formDataDto.MufAssocId, member => member.Ignore())
                .ReverseMap();

            expression.CreateMap<FormData, FormDataPageDTO>()
                .ForMember(formDataPageDto => formDataPageDto.Username, member => member.MapFrom(source => source.CreatedBy.UserName))
                .ForMember(formDataPageDto => formDataPageDto.Version, member => member.MapFrom<FormVersionResolver<FormDataPageDTO>>())
                .ReverseMap();

            expression.CreateMap<FormData, FormSurveyDataDTO>()
                .ForMember(formSurveyDataDto => formSurveyDataDto.RespondentId, member => member
                    .MapFrom(formData => formData.UserId))
                .ForMember(formSurveyDataDto => formSurveyDataDto.RespondentFullName, member => member
                    .MapFrom(formData => $"{formData.CreatedBy.FirstName} {formData.CreatedBy.LastName}"))
                .ForMember(formSurveyDataDto => formSurveyDataDto.RespondentUserName, member => member
                    .MapFrom(formData => formData.CreatedBy.UserName))
                .ForMember(formSurveyDataDto => formSurveyDataDto.FormRevisionJson, member => member
                    .MapFrom<FormVersionResolver<FormSurveyDataDTO>>())
                .ForMember(formSurveyDataDto => formSurveyDataDto.DraftId, member => member.Ignore())
                .ForMember(formSurveyDataDto => formSurveyDataDto.IsMUF, member => member.Ignore())
                .ForMember(formSurveyDataDto => formSurveyDataDto.MultiUserFormAssocLinkId, member => member.Ignore())
                .ForMember(formSurveyDataDto => formSurveyDataDto.RequestId, member => member.Ignore())
                .ForMember(formSurveyDataDto => formSurveyDataDto.MufAssocId, member => member.Ignore());
        }
    }

    public class FormVersionResolver<TFormDataDTO> : IValueResolver<FormData, TFormDataDTO, string> where TFormDataDTO : IDTO
    {
        public string Resolve(FormData source, TFormDataDTO destination, string destMember, ResolutionContext context)
        {
            var revision = source.FormDefinition?.FormRevisions.FirstOrDefault(revision => revision.Id == source.FormDefinition.ActiveRevisionId);
            return revision == null ? string.Empty : $"{revision.MajorVersion}.{revision.MinorVersion}";
        }
    }
}