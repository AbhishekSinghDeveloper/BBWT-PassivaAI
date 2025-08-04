using BBWM.Reporting.DTO;
using BBWM.Reporting.Model;

namespace BBWM.Reporting.Interfaces;

public interface IViewBuilderService
{
    Task<FilterControl> AddFilterControl(int viewId, FilterControlDTO dto, CancellationToken ct = default);
    Task<(GridViewColumn, IList<GridViewColumn>)> AddGridViewColumn(int gridViewId, int queryTableColumnId, Query relatedQuery, CancellationToken ct = default);
    Task<IList<GridViewColumn>> AddGridViewColumns(int gridViewId, int queryTableId, CancellationToken ct = default);
    Task<IEnumerable<QueryFilterBindingDTO>> BindFilterControlToNewQueryFilters(
        IEnumerable<QueryFilterBindingDTO> bindings,
        FilterControl filterControl,
        CancellationToken ct = default);
    Task<QueryFilterBinding> BindFilterControlToQueryFilter(int filterControlId, int queryFilterId, CancellationToken ct = default);
    Task DeleteFilterControl(int filterControlId, bool deleteLinkedQueryFilters, CancellationToken ct = default);
    Task DeleteGridViewColumn(int gridViewColumnId, CancellationToken ct = default);
    Task DeleteQueryFilterBinding(int filterControlBindingId, CancellationToken ct = default);
    Task<SectionDisplayViewDTO> GetSectionView(Section section, CancellationToken ct = default);
    Task ToggleGridViewColumnsSortable(IEnumerable<GridViewColumn> gridViewColumns, bool value, CancellationToken ct = default);
    Task ToggleGridViewColumnsVisible(IEnumerable<GridViewColumn> gridViewColumns, bool value, CancellationToken ct = default);
    Task<FilterControl> UpdateFilterControl(int filterControlId, FilterControlDTO dto, CancellationToken ct = default);
    Task<GridView> UpdateGridView(int gridViewId, GridViewDTO dto, CancellationToken ct = default);
    Task<GridViewColumn> UpdateGridViewColumn(int gridViewColumnId, GridViewColumnDTO dto, CancellationToken ct = default);
}
