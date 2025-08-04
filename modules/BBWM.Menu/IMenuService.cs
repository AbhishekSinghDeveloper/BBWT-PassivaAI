using BBWM.Menu.DTO;

namespace BBWM.Menu;

public interface IMenuReadService
{
    Task<List<MenuDTO>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<List<MenuDTO>> GetForUser(string userId, CancellationToken cancellationToken = default);
}

public interface IMenuEditService
{

    /// <summary>
    /// Update menu
    /// </summary>
    /// <returns>Id</returns>
    Task<int> Update(MenuDTO item, CancellationToken cancellationToken);

    /// <summary>
    /// Create menu
    /// </summary>
    /// <returns>Id</returns>
    Task<int> Create(MenuDTO item, CancellationToken cancellationToken);

    /// <summary>
    /// Update menu by id
    /// </summary>
    /// <returns>boolean</returns>
    Task<bool> Delete(int id, CancellationToken cancellationToken);
}

public interface IMenuService : IMenuReadService, IMenuEditService
{
}
