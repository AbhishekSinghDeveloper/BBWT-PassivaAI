using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Membership.Model;
using BBWM.FormIO.DTO;

namespace BBWM.FormIO.Models
{
    public class FormRequest : IEntity
    {
        public int Id { get; set; }
        public DateTimeOffset RequestDate { get; set; }
        public DateTimeOffset? CompletionDate { get; set; }

        // Foreign keys and navigational properties.
        public int? FormDataId { get; set; }
        public FormData? FormData { get; set; }

        public int FormRevisionId { get; set; }
        public FormRevision? FormRevision { get; set; }

        public string? RequesterId { get; set; }
        public User? Requester { get; set; }

        public static void RegisterMap(IMapperConfigurationExpression expression)
        {
            expression.CreateMap<FormRequest, FormRequestDTO>()
                .ForMember(formRequestDto => formRequestDto.Completed, member => member
                    .MapFrom(formRequest => formRequest.CompletionDate != null))
                .ForMember(formRequestDto => formRequestDto.GroupsIds, member => member.Ignore())
                .ForMember(formRequestDto => formRequestDto.UserIds, member => member.Ignore())
                .ReverseMap();

            expression.CreateMap<FormRequest, FormRequestPageDTO>()
                .ForMember(formRequestPageDto => formRequestPageDto.Completed, member => member
                    .MapFrom(formRequest => formRequest.CompletionDate != null))
                .ForMember(formRequestPageDto => formRequestPageDto.GroupsIds, member => member.Ignore())
                .ForMember(formRequestPageDto => formRequestPageDto.UserIds, member => member.Ignore());
        }
    }
}