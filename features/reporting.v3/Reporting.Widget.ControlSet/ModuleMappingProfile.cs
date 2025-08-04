using System.Text.Json.Nodes;
using AutoMapper;
using BBF.Reporting.Widget.ControlSet.DbModel;
using BBF.Reporting.Widget.ControlSet.DTO;
using BBWM.Core.Utils;

namespace BBF.Reporting.Widget.ControlSet;

public class ModuleMappingProfile : Profile
{
    public ModuleMappingProfile()
    {
        CreateMap<WidgetControlSet, ControlSetViewDTO>().ReverseMap();

        CreateMap<WidgetControlSetItem, ControlSetViewItemDTO>()
            .ForMember(controlSetItemDto => controlSetItemDto.ExtraSettings, member => member
                .MapFrom(controlSetItem => JsonNode.Parse(string.IsNullOrWhiteSpace(controlSetItem.ExtraSettings)
                    ? "{}"
                    : controlSetItem.ExtraSettings, default, default)))
            .ForMember(controlSetItemDto => controlSetItemDto.VariableName, member => member
                .MapFrom(controlSetItem => controlSetItem.Variable == null ? null : controlSetItem.Variable.Name))
            .ReverseMap()
            .ForMember(controlSetItem => controlSetItem.ExtraSettings, member => member
                .MapFrom(controlSetItemDto => controlSetItemDto.ExtraSettings.ToJsonString(JsonSerializerOptionsProvider.Options)))
            .ForMember(controlSetItem => controlSetItem.ControlSet, member => member.Ignore());

        CreateMap<WidgetControlSet, ControlSetDisplayViewDTO>();

        CreateMap<WidgetControlSetItem, ControlSetDisplayViewItemDTO>()
            .ForMember(controlSetItemDto => controlSetItemDto.ExtraSettings, member => member
                .MapFrom(controlSetItem => JsonNode.Parse(string.IsNullOrWhiteSpace(controlSetItem.ExtraSettings)
                    ? "{}"
                    : controlSetItem.ExtraSettings, default, default)))
            .ForMember(controlSetItemDto => controlSetItemDto.VariableName, member => member
                .MapFrom(controlSetItem => controlSetItem.Variable == null ? null : controlSetItem.Variable.Name));
    }
}