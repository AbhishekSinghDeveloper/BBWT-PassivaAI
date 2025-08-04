using BBF.Reporting.Widget.Html.DbModel;
using BBF.Reporting.Widget.Html.DTO;
using BBF.Reporting.Widget.Html.Interfaces;
using BBWM.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace BBF.Reporting.Widget.Html.Services;

public class WidgetHtmlViewService : IWidgetHtmlViewService
{
    private readonly IDataService _dataService;

    public WidgetHtmlViewService(IDataService dataService)
        => _dataService = dataService;

    public async Task<HtmlViewDTO> GetView(Guid widgetSourceId, CancellationToken ct = default)
        => await _dataService.Get<WidgetHtml, HtmlViewDTO>(query => query
            .Where(chart => widgetSourceId == chart.WidgetSourceId)
            .Include(chart => chart.WidgetSource)
            .ThenInclude(source => source.DisplayRule), ct);
}