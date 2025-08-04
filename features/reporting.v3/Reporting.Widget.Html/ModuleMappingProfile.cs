using AutoMapper;
using BBF.Reporting.Widget.Html.DbModel;
using BBF.Reporting.Widget.Html.DTO;

namespace BBF.Reporting.Widget.Html;

public class ModuleMappingProfile : Profile
{
    public ModuleMappingProfile()
    {
        CreateMap<WidgetHtml, HtmlDTO>()
            .ReverseMap()
            .ForMember(html => html.WidgetSourceId, member => member.Ignore())
            .ForMember(html => html.WidgetSource, member => member.Ignore());

        CreateMap<WidgetHtml, HtmlViewDTO>();
    }
}