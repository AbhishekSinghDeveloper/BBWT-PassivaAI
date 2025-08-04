using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.DTO;
using BBF.Reporting.Core.Interfaces;
using BBWM.Core.Exceptions;
using BBWM.Core.Services;
using Microsoft.EntityFrameworkCore;

namespace BBF.Reporting.Core.Services;

public class WidgetSourceService : IWidgetSourceService
{
    private readonly IDataService _dataService;
    private readonly ILoggedUserService _loggedUserService;

    public WidgetSourceService(IDataService dataService,
        ILoggedUserService loggedUserService)
    {
        _dataService = dataService;
        _loggedUserService = loggedUserService;
    }

    public async Task<WidgetSourceDTO> Create(WidgetSourceDTO dto, CancellationToken ct = default)
    {
        dto.Id = Guid.Empty;
        dto.CreatedOn = DateTime.Now;

        dto.DisplayRuleId = null;
        if (dto.DisplayRule != null) dto.DisplayRule.Id = 0;

        await ValidateWidgetSource(dto, ct);

        return await _dataService.Create<WidgetSource, WidgetSourceDTO>(dto, ct);
    }

    public Task<WidgetSourceDTO> Create(WidgetSourceDTO dto, string widgetType, string? ownerId = null,
        CancellationToken ct = default)
    {
        dto.IsDraft = false;
        dto.ReleaseWidgetId = null;
        dto.WidgetType = widgetType;
        dto.OwnerId = ownerId ?? GetLoggedUserId();

        return Create(dto, ct);
    }

    public async Task<WidgetSourceDTO> CreateDraft(WidgetSourceDTO dto, string widgetType, Guid? releaseWidgetId = null,
        CancellationToken ct = default)
    {
        dto.IsDraft = true;
        dto.WidgetType = widgetType;
        dto.OwnerId = GetLoggedUserId();
        dto.ReleaseWidgetId = releaseWidgetId;

        if (releaseWidgetId == null) return await Create(dto, ct);

        // Search for this released widget.
        var releaseWidget = await _dataService.Context.Set<WidgetSource>()
            .FirstOrDefaultAsync(widget => widget.Id == releaseWidgetId, ct);

        if (releaseWidget == null) throw new BusinessException("There is no released widget with the given ID");
        if (releaseWidget.IsDraft) throw new BusinessException("Widget source specified as release cannot be a draft.");

        return await Create(dto, ct);
    }

    public async Task<Guid> ReleaseDraft(Guid widgetSourceId, CancellationToken ct = default)
    {
        var draftSource = await _dataService.Context.Set<WidgetSource>()
                              .FirstOrDefaultAsync(source => source.Id == widgetSourceId, ct)
                          ?? throw new ObjectNotExistsException("Widget source with specified ID doesn't exist.");

        if (!draftSource.IsDraft) return widgetSourceId;
        var releaseWidgetId = draftSource.ReleaseWidgetId;

        // If there is no released version of this widget, release this draft;
        if (releaseWidgetId == null)
        {
            draftSource.IsDraft = false;
            await _dataService.Context.SaveChangesAsync(ct);
            return widgetSourceId;
        }

        // Otherwise, get released version of this widget.
        var releaseSource = await _dataService.Context.Set<WidgetSource>()
                                .Include(source => source.DisplayRule)
                                .FirstOrDefaultAsync(source => source.Id == releaseWidgetId, ct)
                            ?? throw new ObjectNotExistsException("Widget source with specified ID doesn't exist.");

        // Copy edited draft fields to released widget.
        releaseSource.Name = draftSource.Name;
        releaseSource.Code = draftSource.Code;
        releaseSource.Title = draftSource.Title;
        releaseSource.DisplayRule = draftSource.DisplayRule;
        releaseSource.DisplayRuleId = draftSource.DisplayRuleId;
        await _dataService.Context.SaveChangesAsync(ct);

        return releaseWidgetId.Value;
    }

    public async Task<WidgetSourceDTO> Update(WidgetSourceDTO dto, CancellationToken ct = default)
    {
        await ValidateWidgetSource(dto, ct);

        return await _dataService.Update<WidgetSource, WidgetSourceDTO, Guid>(dto, beforeSave: (source, context) =>
        {
            var entry = context.Entry(source);
            var widgetType = entry.OriginalValues.GetValue<string>(nameof(source.WidgetType));
            var displayRuleId = entry.OriginalValues.GetValue<int?>(nameof(source.DisplayRuleId));

            // Prevent widget type to change via update.
            dto.WidgetType = widgetType;

            if (dto.DisplayRule != null || displayRuleId == null) return;

            // Remove display rule if necessary.
            var variableRule = context.Set<VariableRule>().Find(displayRuleId);
            if (variableRule != null) context.Entry(variableRule).State = EntityState.Deleted;
        }, ct);
    }

    public async Task Delete(Guid id, CancellationToken ct = default)
    {
        // Delete all the drafts that has this widget as released widget.
        await DeleteDrafts(id, ct);

        // Delete this widget.
        await _dataService.Delete<WidgetSource, Guid>(id, ct);
    }

    private Task DeleteDrafts(Guid releaseWidgetId, CancellationToken ct = default)
        => _dataService.DeleteAll<WidgetSource>(query => query
            .Where(source => source.ReleaseWidgetId == releaseWidgetId), ct);

    private async Task ValidateWidgetSource(WidgetSourceDTO dto, CancellationToken ct = default)
    {
        if (!string.IsNullOrEmpty(dto.Title))
            ValidateWidgetTitle(dto.Title);

        if (!string.IsNullOrEmpty(dto.Name))
        {
            dto.Name = dto.Name.Trim();
            await ValidateWidgetName(dto.Name, dto.IsDraft ? dto.ReleaseWidgetId : dto.Id, ct);
        }

        if (!string.IsNullOrEmpty(dto.Code))
        {
            dto.Code = dto.Code.Trim();
            await ValidateWidgetCode(dto.Code, dto.IsDraft ? dto.ReleaseWidgetId : dto.Id, ct);
        }
    }

    private static void ValidateWidgetTitle(string title)
    {
        if (title.Length > 500) throw new BusinessException("Widget title cannot be longer than 500 characters.");
    }

    private async Task ValidateWidgetName(string name, Guid? excludeId, CancellationToken ct)
    {
        if (name.Length > 500) throw new BusinessException("Widget name cannot be longer than 500 characters.");

        var unique = await _dataService.Context.Set<WidgetSource>().AllAsync(source =>
            source.Name == null || source.IsDraft || source.Id == excludeId || !EF.Functions.Like(name, source.Name), ct);

        if (!unique) throw new BusinessException($"Another widget with the same name '{name}' already exists.");
    }

    private async Task ValidateWidgetCode(string code, Guid? excludeId, CancellationToken ct)
    {
        if (code.Length > 200) throw new BusinessException("Widget code cannot be longer than 200 characters.");

        var unique = await _dataService.Context.Set<WidgetSource>().AllAsync(source =>
            source.Code == null || source.IsDraft || source.Id == excludeId || !EF.Functions.Like(code, source.Code), ct);

        if (!unique) throw new BusinessException($"Another widget with the same code '{code}' already exists.");
    }

    private string GetLoggedUserId()
        => _loggedUserService.GetLoggedUserId()
           ?? throw new BusinessException("Cannot get user ID to set widget source owner.");
}