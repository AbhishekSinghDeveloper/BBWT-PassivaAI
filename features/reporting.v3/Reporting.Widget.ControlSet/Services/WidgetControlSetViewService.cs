using AutoMapper;
using BBF.Reporting.Widget.ControlSet.DbModel;
using BBF.Reporting.Widget.ControlSet.DTO;
using BBF.Reporting.Widget.ControlSet.Interfaces;
using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace BBF.Reporting.Widget.ControlSet.Services;

public class WidgetControlSetViewService : IWidgetControlSetViewService
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;


    public WidgetControlSetViewService(IMapper mapper, IDbContext context)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<ControlSetDisplayViewDTO> GetView(Guid widgetSourceId, CancellationToken ct = default)
    {
        var widgetControlSet = await _context.Set<WidgetControlSet>()
            .Include(controlSet => controlSet.WidgetSource).ThenInclude(source => source.DisplayRule)
            .Include(controlSet => controlSet.Items).ThenInclude(item => item.Variable)
            .Include(controlSet => controlSet.Items).ThenInclude(item => item.FilterRule)
            .FirstOrDefaultAsync(controlSet => controlSet.WidgetSourceId == widgetSourceId, ct);

        if (widgetControlSet == null)
            throw new ObjectNotExistsException("The widget source with specified ID doesn't exist.");

        widgetControlSet.Items = widgetControlSet.Items.OrderBy(controlSetItem => controlSetItem.SortOrder).ToList();

        return _mapper.Map<ControlSetDisplayViewDTO>(widgetControlSet);
    }
}