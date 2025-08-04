using BBWM.Menu.DTO;

namespace BBWM.Menu;

public interface IMenuDataProvider
{
    Task<List<MenuDTO>> GetAll(CancellationToken cancellationToken = default);

    Task AddRange(IEnumerable<MenuDTO> items, CancellationToken cancellationToken = default);

    Task<bool> UpdateRange(IEnumerable<MenuDTO> menu, CancellationToken cancellationToken = default);

    Task<int> Create(MenuDTO menu, CancellationToken cancellationToken = default);

    Task<bool> Delete(int id, CancellationToken cancellationToken = default);

    int GetMaxIndex(int? parentId = null);
}
