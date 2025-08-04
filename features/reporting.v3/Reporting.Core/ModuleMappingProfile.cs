using AutoMapper;
using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.DTO;

namespace BBF.Reporting.Core;

public class ModuleMappingProfile : Profile
{
    public ModuleMappingProfile()
    {
        CreateMap<QuerySource, QuerySourceDTO>()
            .ForMember(querySourceDto => querySourceDto.OwnerName, member => member
                .MapFrom(querySource => querySource.Owner != null ? querySource.Owner.UserName : null))
            .ForMember(querySourceDto => querySourceDto.OrganizationIds, member => member
                .MapFrom(querySource => querySource.Organizations.Select(organization => organization.Id)))
            .ReverseMap();

        CreateMap<WidgetSource, WidgetSourceDTO>()
            .ForMember(widgetSourceDto => widgetSourceDto.OwnerName, member => member
                .MapFrom(widgetSource => widgetSource.Owner != null ? widgetSource.Owner.UserName : null))
            .ForMember(widgetSourceDto => widgetSourceDto.OrganizationIds, member => member
                .MapFrom(widgetSource => widgetSource.Organizations.Select(organization => organization.Id)))
            .ReverseMap();

        CreateMap<WidgetSource, WidgetSourcePreloadDTO>();

        CreateMap<Variable, VariableDTO>().ReverseMap();
        CreateMap<VariableRule, VariableRuleDTO>().ReverseMap();
        CreateMap<FilterRule, FilterRuleDTO>().ReverseMap();
    }
}