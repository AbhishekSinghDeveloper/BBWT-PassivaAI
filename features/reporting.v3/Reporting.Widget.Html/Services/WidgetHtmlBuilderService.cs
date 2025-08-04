using BBF.Reporting.Core.DTO;
using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Widget.Html.DbModel;
using BBF.Reporting.Widget.Html.DTO;
using BBF.Reporting.Widget.Html.Interfaces;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;

namespace BBF.Reporting.Widget.Html.Services;

public class WidgetHtmlBuilderService : IWidgetHtmlBuilderService
{
    private readonly IDataService _dataService;
    private readonly IWidgetHtmlProvider _widgetHtmlProvider;
    private readonly IWidgetSourceService _widgetSourceService;

    public WidgetHtmlBuilderService(
        IDataService dataService,
        IWidgetHtmlProvider widgetHtmlProvider,
        IWidgetSourceService widgetSourceService)
    {
        _dataService = dataService;
        _widgetHtmlProvider = widgetHtmlProvider;
        _widgetSourceService = widgetSourceService;
    }

    public Task<HtmlDTO> Create(HtmlDTO build, CancellationToken ct = default)
        => Create(build, null, ct);

    public async Task<HtmlDTO> Create(HtmlDTO build, string? userId, CancellationToken ct = default)
    {
        // Create new source for this html.
        const string widgetType = WidgetHtmlProvider.SourceType;
        var source = await _widgetSourceService.Create(build.WidgetSource, widgetType, userId, ct)
                     ?? throw new BusinessException("Cannot create widget source for this html widget");

        return await CreateHtml(source, build, ct);
    }

    public async Task<HtmlDTO> CreateDraft(HtmlDTO build, Guid? releaseWidgetId = null, CancellationToken ct = default)
    {
        // Create new source for this html.
        const string widgetType = WidgetHtmlProvider.SourceType;
        var source = await _widgetSourceService.CreateDraft(build.WidgetSource, widgetType, releaseWidgetId, ct)
                     ?? throw new BusinessException("Cannot create widget source for this html widget");

        return await CreateHtml(source, build, ct);
    }

    private async Task<HtmlDTO> CreateHtml(WidgetSourceDTO source, HtmlDTO build, CancellationToken ct = default)
    {
        // Create html and assign it to this source.
        build.Id = 0;
        return await _dataService.Create<WidgetHtml, HtmlDTO>(build,
            beforeSave: (widgetHtml, _) => { widgetHtml.WidgetSourceId = source.Id; }, ct);
    }

    public Task<Guid> ReleaseDraft(Guid widgetSourceId, CancellationToken ct = default)
        => _widgetHtmlProvider.ReleaseDraft(widgetSourceId, ct);

    public async Task<HtmlDTO> Update(HtmlDTO build, CancellationToken ct = default)
    {
        // Update source if it's necessary.
        await _widgetSourceService.Update(build.WidgetSource, ct);

        // Update this html.
        return await _dataService.Update<WidgetHtml, HtmlDTO>(build, ct);
    }
}