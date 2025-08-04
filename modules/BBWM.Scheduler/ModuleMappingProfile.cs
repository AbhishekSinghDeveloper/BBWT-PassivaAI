using AutoMapper;
using BBWM.Scheduler.DTO;
using BBWM.Scheduler.Model;

namespace BBWM.Scheduler;

public class ModuleMappingProfile : Profile
{
    public ModuleMappingProfile()
    {
        CreateMap<JobRunDetails, JobRunDetailsDTO>()
                    .ForMember(dest => dest.TimeUntilDeletion, opt => opt.MapFrom(src =>
                        src.Duration.HasValue
                            ? (src.Duration.Value - (DateTimeOffset.Now - new DateTimeOffset(src.LastModified)))
                                .ToString(@"dd\.hh\:mm\:ss")
                            : null))
                    .ForMember(dest => dest.MinutesSinceLastModified, opt => opt.MapFrom(src =>
                        (DateTimeOffset.Now - new DateTimeOffset(src.LastModified)).TotalMinutes.ToString("F0")))
                    .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                    .ForMember(dest => dest.JobName, opt => opt.MapFrom(src => src.JobName))
                    .ForMember(dest => dest.LastModified, opt => opt.MapFrom(src => new DateTimeOffset(src.LastModified)))
                    .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                    .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
                    .ForMember(dest => dest.ServerName, opt => opt.MapFrom(src => src.ServerName))
                    .ForMember(dest => dest.IsRecurring, opt => opt.MapFrom(src => src.IsRecurring));
    }
}

