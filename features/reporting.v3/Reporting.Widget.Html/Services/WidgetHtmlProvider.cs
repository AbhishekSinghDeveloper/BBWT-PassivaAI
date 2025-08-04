using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Widget.Html.DbModel;
using BBF.Reporting.Widget.Html.Interfaces;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BBF.Reporting.Widget.Html.Services;

public class WidgetHtmlProvider : IWidgetHtmlProvider
{
    public const string SourceType = "html";

    private readonly IDbContext _context;
    private readonly IWidgetSourceService _widgetSourceService;

    public WidgetHtmlProvider(IDbContext context,
        IWidgetSourceService widgetSourceService)
    {
        _context = context;
        _widgetSourceService = widgetSourceService;
    }

    public Task<bool> HasAttachedWidgets(Guid querySourceId, CancellationToken ct = default)
        => Task.FromResult(false);

    public Task<IEnumerable<WidgetSource>> GetAttachedWidgets(Guid querySourceId, CancellationToken ct = default)
        => Task.FromResult(Enumerable.Empty<WidgetSource>());

    public async Task<Guid> ReleaseDraft(Guid widgetSourceId, CancellationToken ct = default)
    {
        var draftHtml = await _context.Set<WidgetHtml>()
                            .FirstOrDefaultAsync(html => html.WidgetSourceId == widgetSourceId, ct)
                        ?? throw new ObjectNotExistsException("Draft html widget with specified ID doesn't exist.");

        var releasedHtmlId = await _widgetSourceService.ReleaseDraft(widgetSourceId, ct);
        if (releasedHtmlId == widgetSourceId) return widgetSourceId;

        var releaseHtml = await _context.Set<WidgetHtml>()
                              .FirstOrDefaultAsync(html => html.WidgetSourceId == releasedHtmlId, ct)
                          ?? throw new ObjectNotExistsException("Released html widget with specified ID doesn't exist.");

        // Copy edited draft fields to released html.
        releaseHtml.InnerHtml = draftHtml.InnerHtml;

        await _context.SaveChangesAsync(ct);

        return releasedHtmlId;
    }
}