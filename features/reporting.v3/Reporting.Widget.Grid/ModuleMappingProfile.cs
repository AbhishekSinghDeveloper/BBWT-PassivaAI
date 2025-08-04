using AutoMapper;
using BBF.Reporting.Widget.Grid.DbModel;
using BBF.Reporting.Widget.Grid.DTO;
using BBWM.Core.Utils;
using System.Text.Json.Nodes;

namespace BBF.Reporting.Widget.Grid;

public class ModuleMappingProfile : Profile
{
    public ModuleMappingProfile()
    {
        CreateMap<WidgetGrid, GridViewDTO>()
            .ReverseMap()
            .ForMember(grid => grid.Columns, member => member.Ignore())
            .ForMember(grid => grid.WidgetSource, member => member.Ignore())
            .ForMember(grid => grid.QuerySource, member => member.Ignore());

        CreateMap<WidgetGridColumn, GridViewColumnDTO>()
            .ForMember(columnDto => columnDto.ExtraSettings, member => member
                .MapFrom(column => JsonNode.Parse(column.ExtraSettings ?? "{}", default, default)))
            .ForMember(columnDto => columnDto.Footer, member => member
                .MapFrom(column => JsonNode.Parse(column.Footer ?? "{}", default, default)))
            .ForMember(columnDto => columnDto.VariableName, member => member
                .MapFrom(column => column.Variable == null ? null : column.Variable.Name))
            .ForMember(columnDto => columnDto.Grid, member => member.Ignore())
            .ReverseMap()
            .ForMember(column => column.ExtraSettings, member => member
                .MapFrom(columnDto => columnDto.ExtraSettings.ToJsonString(JsonSerializerOptionsProvider.Options)))
            .ForMember(column => column.Footer, member => member
                .MapFrom(columnDto => columnDto.Footer.ToJsonString(JsonSerializerOptionsProvider.Options)))
            .ForMember(column => column.Variable, member => member.Ignore())
            .ForMember(column => column.Grid, member => member.Ignore());

        CreateMap<WidgetGrid, GridDisplayViewDTO>();

        CreateMap<WidgetGridColumn, GridDisplayViewColumnDTO>()
            .ForMember(columnDto => columnDto.ExtraSettings, member => member
                .MapFrom(column => JsonNode.Parse(column.ExtraSettings ?? "{}", default, default)))
            .ForMember(columnDto => columnDto.Footer, member => member
                .MapFrom(column => JsonNode.Parse(column.Footer ?? "{}", default, default)))
            .ForMember(columnDto => columnDto.VariableName, member => member
                .MapFrom(column => column.Variable == null ? null : column.Variable.Name));
    }
}