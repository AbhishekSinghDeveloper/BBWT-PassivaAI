using AutoMapper;
using BBF.Reporting.Widget.Chart.DbModel;
using BBF.Reporting.Widget.Chart.DTO;

namespace BBF.Reporting.Widget.Chart;

public class ModuleMappingProfile : Profile
{
    public ModuleMappingProfile()
    {
        CreateMap<WidgetChart, ChartBuildDTO>()
            .ReverseMap()
            .ForMember(chart => chart.WidgetSourceId, member => member.Ignore())
            .ForMember(chart => chart.WidgetSource, member => member.Ignore());

        CreateMap<WidgetChartColumn, ChartBuildColumnDTO>()
            .ReverseMap()
            .ForMember(column => column.ChartId, member => member.Ignore())
            .ForMember(column => column.Chart, member => member.Ignore());

        CreateMap<WidgetChart, ChartViewDTO>();

        CreateMap<WidgetChartColumn, ChartViewColumnDTO>();
    }
}