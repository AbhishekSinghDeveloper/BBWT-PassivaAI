using AutoMapper;
using BBF.Reporting.Dashboard.DbModel;
using BBF.Reporting.Dashboard.DTO;

namespace BBF.Reporting.Dashboard;

public class ModuleMappingProfile : Profile
{
    public ModuleMappingProfile()
    {
        CreateMap<DbModel.Dashboard, DashboardDTO>()
            .ForMember(dashboardView => dashboardView.OwnerName, member => member
                .MapFrom(dashboard => dashboard.Owner != null ? dashboard.Owner.UserName : null))
            .ForMember(dashboardView => dashboardView.OrganizationIds, member => member
                .MapFrom(dashboard => dashboard.Organizations.Select(organization => organization.Id)));

        CreateMap<DbModel.Dashboard, DashboardBuildDTO>()
            .ReverseMap()
            .ForMember(dashboard => dashboard.Owner, member => member.Ignore())
            .ForMember(dashboard => dashboard.Organizations, member => member.Ignore());

        CreateMap<DashboardWidget, DashboardBuildWidgetDTO>()
            .ReverseMap()
            .ForMember(dashboard => dashboard.WidgetSource, member => member.Ignore())
            .ForMember(dashboard => dashboard.DashboardId, member => member.Ignore())
            .ForMember(dashboard => dashboard.Dashboard, member => member.Ignore());

        CreateMap<DbModel.Dashboard, DashboardViewDTO>();

        CreateMap<DashboardWidget, DashboardViewWidgetDTO>()
            .ForMember(widgetView => widgetView.WidgetType, member => member
                .MapFrom(widget => widget.WidgetSource.WidgetType));
    }
}