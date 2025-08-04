using AutoMapper;
using BBWM.Core.Data;
using BBWM.FormIO.DTO;

namespace BBWM.FormIO.Models
{
    public class FormSurvey : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public DateTimeOffset Created { get; set; }

        // Foreign keys and navigational properties.
        public int? FormRevisionId { get; set; }
        public FormRevision? FormRevision { get; set; }
        public ICollection<FormData> SurveyFormDataInstances { get; set; } = new List<FormData>();

        public static void RegisterMap(IMapperConfigurationExpression expression)
        {
            expression.CreateMap<FormSurvey, FormSurveyDTO>()
                .ForMember(formSurveyDto => formSurveyDto.FormRevisionId, member => member
                    .MapFrom(formSurvey => formSurvey.FormRevisionId))
                .ForMember(formSurveyDto => formSurveyDto.SurveyFormDataInstances, member => member
                    .MapFrom(formSurvey => formSurvey.SurveyFormDataInstances))
                .ForMember(formSurveyDto => formSurveyDto.SurveyedUsers, member => member.Ignore())
                .ReverseMap()
                .ForMember(formSurvey => formSurvey.FormRevisionId, member => member
                    .MapFrom(formSurveyDto => formSurveyDto.FormRevisionId))
                .ForMember(formSurvey => formSurvey.FormRevision, member => member.Ignore());


            expression.CreateMap<FormSurvey, FormSurveyPageDTO>()
                .ForMember(formSurveyPageDto => formSurveyPageDto.FormRevisionId, member => member
                    .MapFrom(formSurvey => formSurvey.FormRevisionId))
                .ForMember(formSurveyPageDto => formSurveyPageDto.SurveyFormDataInstances, member => member
                    .MapFrom(formSurvey => formSurvey.SurveyFormDataInstances))
                .ForMember(formSurveyPageDto => formSurveyPageDto.FormDefinitionName, member => member
                    .MapFrom(formSurvey => formSurvey.FormRevision!.FormDefinition!.Name))
                .ForMember(formSurveyPageDto => formSurveyPageDto.Version, member => member
                    .MapFrom(formSurvey => $"{formSurvey.FormRevision!.MajorVersion}.{formSurvey.FormRevision.MinorVersion}"))
                .ForMember(formSurveyPageDto => formSurveyPageDto.SurveyedUsers, member => member.Ignore())
                .ForMember(formSurveyPageDto => formSurveyPageDto.Orgs, member => member
                    .MapFrom(formSurvey => formSurvey.FormRevision!.FormDefinition!.FormDefinitionOrganizations.Any()
                        ? formSurvey.FormRevision!.FormDefinition!.FormDefinitionOrganizations
                            .Select(organization => organization.Organization.Name)
                            .Aggregate((current, next) => current + ", " + next)
                        : "-"));

            expression.CreateMap<FormSurvey, FormSurveyMinDTO>();
        }
    }
}