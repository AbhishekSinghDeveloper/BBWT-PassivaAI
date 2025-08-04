using Amazon.EventBridge;

using AutoMapper;

using BBWM.AWS.EventBridge.DTO;

using System.Text.RegularExpressions;

namespace BBWM.AWS.EventBridge.Mapping;

public class AwsEventBridgeMappingProfile : Profile
{
    private static readonly Regex cronRegex = new Regex(@"^(cron\((?<cron>[^)]+)\)|)$");

    public AwsEventBridgeMappingProfile()
    {
        CreateMap<Amazon.EventBridge.Model.Rule, AwsEventBridgeRuleDTO>()
            .ForMember(dto => dto.Id, conf => conf.MapFrom(m => m.Name)) // Name is unique
            .ForMember(dto => dto.Name, conf => conf.MapFrom(m => m.Name))
            .ForMember(dto => dto.Cron, conf => conf.MapFrom(m => GetCron(m.ScheduleExpression)))
            .ForMember(dto => dto.IsEnabled, conf => conf.MapFrom(m => m.State == RuleState.ENABLED))
            .ForMember(dto => dto.LastExecutionTime, conf => conf.Ignore())
            .ForMember(dto => dto.NextExecutionTime, conf => conf.Ignore())
            .ForMember(dto => dto.TimeZoneId, conf => conf.Ignore())
            .ForMember(dto => dto.TargetJobId, conf => conf.Ignore())
            .ForMember(dto => dto.Parameters, conf => conf.Ignore());

        CreateMap<AwsEventBridgeRuleDTO, Amazon.EventBridge.Model.PutRuleRequest>()
            .ForMember(req => req.Name, conf => conf.MapFrom(m => m.Name))
            .ForMember(req => req.ScheduleExpression, dto => dto.MapFrom(m => m.GetAwsCron()))
            .ForMember(
                req => req.State,
                conf => conf.MapFrom(m => m.IsEnabled ? RuleState.ENABLED : RuleState.DISABLED))
            .ForMember(req => req.Description, conf => conf.Ignore())
            .ForMember(req => req.EventBusName, conf => conf.Ignore())
            .ForMember(req => req.EventPattern, conf => conf.Ignore())
            .ForMember(req => req.RoleArn, conf => conf.Ignore())
            .ForMember(req => req.Tags, conf => conf.Ignore());
    }

    private static string GetCron(string scheduleExpression)
    {
        scheduleExpression ??= "";
        var m = cronRegex.Match(scheduleExpression);
        return m.Success
            ? m.Groups["cron"].Value
            : default;
    }
}
