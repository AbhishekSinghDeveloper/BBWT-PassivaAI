using BBWM.Core.Filters;
using BBWM.Reporting.DTO;

namespace BBWM.Reporting.Interfaces;

public interface ISectionViewService
{
    Task<SectionDisplayViewDTO> GetDisplayView(Guid sectionId, CancellationToken ct = default);

    Task<IEnumerable<DropDownOption>> GetFilterOptions(Guid sectionId, int filterControlId, CancellationToken ct = default);

    Task<IEnumerable<dynamic>> GetData(Guid sectionId, QueryCommand queryCommand, CancellationToken ct = default);

    Task<int> GetTotal(Guid sectionId, QueryCommand queryCommand = null, CancellationToken ct = default);

    Task<dynamic> GetAggregations(Guid sectionId, QueryCommand queryCommand = null, CancellationToken ct = default);
}