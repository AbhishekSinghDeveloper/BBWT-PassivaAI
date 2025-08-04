using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Membership.Model;
using BBWM.FormIO.DTO;

namespace BBWM.FormIO.Models
{
    public class FormRevision : IEntity
    {
        public int Id { get; set; }

        public int MajorVersion { get; set; }

        public int MinorVersion { get; set; }

        public DateTimeOffset DateCreated { get; set; }

        /// <summary>
        /// Indicates if a form can be filled on mobile devices
        /// </summary>
        public bool MobileFriendly { get; set; } = false;

        public string? CreatorId { get; set; }
        public User? Creator { get; set; }

        public string? Note { get; set; }

        public int? FormDefinitionId { get; set; }
        public FormDefinition? FormDefinition { get; set; }

        public string? Json { get; set; }

        public bool? MUFCapable { get; set; }

        public ICollection<FormSurvey> Surveys { get; set; } = new List<FormSurvey>();

        public static void RegisterMap(IMapperConfigurationExpression c)
        {
            c.CreateMap<FormRevision, FormRevisionDTO>()
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(x => x.Creator.UserName))
                .ReverseMap()
                .ForMember(dest => dest.Creator, opt => opt.Ignore());

            c.CreateMap<FormRevision, FormRevisionMinDTO>()
                .ReverseMap();

            c.CreateMap<FormRevision, InitialFormRevisionRequestDTO>()
                .ReverseMap()
                .ForMember(dest => dest.Creator, opt => opt.Ignore());

            c.CreateMap<FormRevision, NewFormRevisionRequestDTO>()
                .ReverseMap()
                .ForMember(dest => dest.Creator, opt => opt.Ignore());

            c.CreateMap<FormRevision, FormRevisionForRequestMinDTO>()
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(x => x.Creator!.UserName))
                .ReverseMap();

            c.CreateMap<FormRevision, FormRevisionForFormDataPageDTO>()
                .ForMember(dest => dest.CreatorName, opt => opt.MapFrom(x => x.Creator!.UserName))
                .ReverseMap();
        }
    }
}