using BBF.Reporting.Core.Model;

namespace BBF.Reporting.Widget.ControlSet.Interfaces;

public interface IWidgetControlSetDataService
{
    Task<IEnumerable<dynamic>> GetDropdownData(QueryDataRequest request, CancellationToken ct = default);
}