using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.DTO;
using BBF.Reporting.Core.Enums;
using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.Widget.ControlSet.DbModel;
using BBF.Reporting.Widget.ControlSet.DTO;
using BBF.Reporting.Widget.ControlSet.Interfaces;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace BBF.Reporting.Widget.ControlSet.Services;

public class WidgetControlSetBuilderService : IWidgetControlSetBuilderService
{
    private readonly IDataService _dataService;
    private readonly IWidgetSourceService _widgetSourceService;
    private readonly IWidgetControlSetProvider _widgetControlSetProvider;

    public WidgetControlSetBuilderService(
        IDataService dataService,
        IWidgetSourceService widgetSourceService,
        IWidgetControlSetProvider widgetControlSetProvider)
    {
        _dataService = dataService;
        _widgetSourceService = widgetSourceService;
        _widgetControlSetProvider = widgetControlSetProvider;
    }

    public async Task<ControlSetViewDTO> GetView(Guid widgetSourceId, CancellationToken ct = default)
    {
        var widgetControlSet = await _dataService.Get<WidgetControlSet, ControlSetViewDTO>(query => query
                                   .Where(controlSet => controlSet.WidgetSourceId == widgetSourceId)
                                   .Include(controlSet => controlSet.WidgetSource).ThenInclude(source => source.DisplayRule)
                                   .Include(controlSet => controlSet.Items).ThenInclude(item => item.Variable)
                                   .Include(controlSet => controlSet.Items).ThenInclude(item => item.FilterRule), ct)
                               ?? throw new ObjectNotExistsException("The widget source with specified ID doesn't exist.");

        return widgetControlSet;
    }

    public Task<ControlSetViewDTO> Create(ControlSetViewDTO build, CancellationToken ct = default)
        => Create(build, null, ct);

    public async Task<ControlSetViewDTO> Create(ControlSetViewDTO build, string? userId, CancellationToken ct = default)
    {
        ValidateControlSet(build);

        // Create new source for this controlSet.
        const string widgetType = WidgetControlSetProvider.SourceType;
        var source = await _widgetSourceService.Create(build.WidgetSource, widgetType, userId, ct)
                     ?? throw new BusinessException("Cannot create widget source for this control set widget");

        return await CreateControlSet(source, build, ct);
    }

    public async Task<ControlSetViewDTO> CreateDraft(ControlSetViewDTO build, Guid? releaseWidgetId = null, CancellationToken ct = default)
    {
        ValidateControlSet(build);

        // Create new source for this control set.
        const string widgetType = WidgetControlSetProvider.SourceType;
        var source = await _widgetSourceService.CreateDraft(build.WidgetSource, widgetType, releaseWidgetId, ct)
                     ?? throw new BusinessException("Cannot create widget source for this control set widget");

        return await CreateControlSet(source, build, ct);
    }

    private async Task<ControlSetViewDTO> CreateControlSet(WidgetSourceDTO source, ControlSetViewDTO build, CancellationToken ct = default)
    {
        // Create control set and assign it to this source.
        build.Id = 0;
        var controlSet = await _dataService.Create<WidgetControlSet, ControlSetViewDTO>(build,
            beforeSave: (widgetControlSet, _) => { widgetControlSet.WidgetSourceId = source.Id; }, ct);

        // Create chart items and assign them to this chart.
        foreach (var buildItem in build.Items)
        {
            buildItem.Id = 0;
            buildItem.FilterRuleId = null;
            if (buildItem.FilterRule != null) buildItem.FilterRule.Id = 0;

            var item = await _dataService.Create<WidgetControlSetItem, ControlSetViewItemDTO>(buildItem,
                beforeSave: (widgetControlSetItem, _) => UpdateControlSetItem(controlSet.Id, widgetControlSetItem, buildItem), ct);
            controlSet.Items.Add(item);
        }

        return controlSet;
    }

    public Task<Guid> ReleaseDraft(Guid widgetSourceId, CancellationToken ct = default)
        => _widgetControlSetProvider.ReleaseDraft(widgetSourceId, ct);

    public async Task<ControlSetViewDTO> Update(ControlSetViewDTO build, CancellationToken ct = default)
    {
        // Update source.
        await _widgetSourceService.Update(build.WidgetSource, ct);

        // Update control set.
        var controlSet = await _dataService.Update<WidgetControlSet, ControlSetViewDTO>(build, ct);

        if (controlSet.Items.DistinctBy(item => item.Name).Count() < controlSet.Items.Count)
            throw new BusinessException("Control set items must have unique names.");

        // Delete all items of this control set that no longer belongs to it.
        var itemIds = build.Items.Select(item => item.Id);
        await _dataService.DeleteAll<WidgetControlSetItem>(query =>
            query.Where(item => item.ControlSetId == build.Id && !itemIds.Contains(item.Id)), ct);

        // Create or update chart items and assign them to this chart.
        foreach (var buildItem in build.Items)
        {
            var item = buildItem.Id == 0
                ? await _dataService.Create<WidgetControlSetItem, ControlSetViewItemDTO>(buildItem,
                    beforeSave: (widgetControlSetItem, _) => UpdateControlSetItem(controlSet.Id, widgetControlSetItem, buildItem), ct)
                : await _dataService.Update<WidgetControlSetItem, ControlSetViewItemDTO>(buildItem,
                    beforeSave: (widgetControlSetItem, _) => UpdateControlSetItem(controlSet.Id, widgetControlSetItem, buildItem), ct);
            controlSet.Items.Add(item);
        }

        return controlSet;
    }

    private void UpdateControlSetItem(int controlSetId, WidgetControlSetItem item, ControlSetViewItemDTO buildItem)
    {
        item.ControlSetId = controlSetId;

        // Get associated variable if exists.
        item.Variable = item.VariableId != null ? _dataService.Context.Set<Variable>().Find(item.VariableId) : null;

        if (buildItem.VariableName is { Length: > 0 } name)
        {
            item.Variable ??= new Variable();
            item.Variable.Name = name;
        }
        else if (item.Variable != null)
        {
            _dataService.Context.Entry(item.Variable).State = EntityState.Deleted;
        }

        var entry = _dataService.Context.Entry(item);
        var filterRuleId = entry.OriginalValues.GetValue<int?>(nameof(item.FilterRuleId));
        if (buildItem.FilterRule != null || filterRuleId == null) return;

        // Remove filter rule if necessary.
        var filterRule = _dataService.Context.Set<FilterRule>().Find(filterRuleId);
        if (filterRule != null) _dataService.Context.Entry(filterRule).State = EntityState.Deleted;
    }

    private static void ValidateControlSet(ControlSetViewDTO build)
    {
        if (build.Items.Any(InvalidDropdownItem))
            throw new BusinessException("Invalid dropdown/multiselect item: table settings are not fully specified");
    }

    private static bool InvalidDropdownItem(ControlSetViewItemDTO item)
        => item.InputType is InputType.Dropdown or InputType.Multiselect && item is not
            { FolderId: not null, SourceCode: not null, TableId: not null, LabelColumnId: not null, ValueColumnId: not null };
}