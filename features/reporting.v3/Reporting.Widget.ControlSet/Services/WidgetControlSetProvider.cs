using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Widget.ControlSet.DbModel;
using BBF.Reporting.Widget.ControlSet.Interfaces;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BBF.Reporting.Widget.ControlSet.Services;

public class WidgetControlSetProvider : IWidgetControlSetProvider
{
    public const string SourceType = "control-set";

    private readonly IDbContext _context;
    private readonly IWidgetSourceService _widgetSourceService;

    public WidgetControlSetProvider(IDbContext context,
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
        var draftControlSet = await _context.Set<WidgetControlSet>()
                                  .Include(controlSet => controlSet.Items).ThenInclude(item => item.Variable)
                                  .FirstOrDefaultAsync(controlSet => controlSet.WidgetSourceId == widgetSourceId, ct)
                              ?? throw new ObjectNotExistsException("Draft control set widget with specified ID doesn't exist.");

        var releasedControlSetId = await _widgetSourceService.ReleaseDraft(widgetSourceId, ct);
        if (releasedControlSetId == widgetSourceId) return widgetSourceId;

        var releaseControlSet = await _context.Set<WidgetControlSet>()
                                    .Include(controlSet => controlSet.Items).ThenInclude(item => item.Variable)
                                    .FirstOrDefaultAsync(controlSet => controlSet.WidgetSourceId == releasedControlSetId, ct)
                                ?? throw new ObjectNotExistsException("Released control set widget with specified ID doesn't exist.");

        // Copy edited draft fields to released control set.
        releaseControlSet.Items = (from item in draftControlSet.Items
            select new WidgetControlSetItem
            {
                Name = item.Name,
                SortOrder = item.SortOrder,
                DataType = item.DataType,
                InputType = item.InputType,

                HintText = item.HintText,
                ExtraSettings = item.ExtraSettings,
                EmptyFilterIfFalse = item.EmptyFilterIfFalse,
                UserCanChangeOperator = item.UserCanChangeOperator,
                ValueEmitType = item.ValueEmitType,

                TableId = item.TableId,
                FolderId = item.FolderId,
                SourceCode = item.SourceCode,
                ValueColumnId = item.ValueColumnId,
                LabelColumnId = item.LabelColumnId,

                VariableId = item.VariableId,
                ControlSetId = releaseControlSet.Id,
                FilterRuleId = item.FilterRuleId,

                Variable = item.Variable,
                ControlSet = releaseControlSet,
                FilterRule = item.FilterRule
            }).ToList();

        await _context.SaveChangesAsync(ct);

        return releasedControlSetId;
    }
}