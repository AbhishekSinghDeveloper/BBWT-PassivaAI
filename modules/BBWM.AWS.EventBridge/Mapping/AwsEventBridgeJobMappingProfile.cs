using AutoMapper;

using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Model;

namespace BBWM.AWS.EventBridge.Mapping;

public class AwsEventBridgeJobMappingProfile : Profile
{
    public AwsEventBridgeJobMappingProfile()
    {
        CreateMap<EventBridgeJobParameter, AwsEventBridgeJobParameterDTO>()
            .ReverseMap();

        CreateMap<EventBridgeJob, AwsEventBridgeJobDTO>()
            .ReverseMap();
        CreateMap<AwsEventBridgeRunningJobDTO, EventBridgeRunningJob>()
            .ReverseMap();
        CreateMap<AwsEventBridgeJobHistoryDTO, EventBridgeJobHistory>()
            .ReverseMap();

        CreateMap<AwsEventBridgeRuleDTO, AwsEventBridgeJobDTO>()
                .ForMember(job => job.JobId, config => config.MapFrom(rule => rule.TargetJobId))
                .ForMember(job => job.RuleId, config => config.MapFrom(rule => rule.Name))
                .ForMember(job => job.TimeZone, config => config.MapFrom(rule => TimeZoneInfo.Utc.Id))
                .ForMember(job => job.Id, config => config.Ignore())
                .ForMember(job => job.NextExecutionTime, config => config.Ignore())
                .ForMember(job => job.LastExecutionTime, config => config.Ignore())
            .ReverseMap()
                .ForMember(rule => rule.Id, config => config.Ignore())
                .ForMember(rule => rule.Name, config => config.Ignore())
                .ForMember(rule => rule.IsEnabled, config => config.Ignore())
                .ForMember(rule => rule.Cron, config => config.Ignore())
                .ForMember(rule => rule.TimeZoneId, config => config.MapFrom(job => job.TimeZone))
                .ForMember(rule => rule.TargetJobId, config => config.MapFrom(job => job.JobId));
    }
}
