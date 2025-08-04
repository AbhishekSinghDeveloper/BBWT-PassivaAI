using BBWM.Menu.DTO;

namespace BBWM.Menu;

public interface IFooterMenuService
{
    Task<bool> Exists(int id, CancellationToken cancellationToken = default);
    Task<List<FooterMenuItemDTO>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<FooterMenuItemDTO> Save(FooterMenuItemDTO item, CancellationToken cancellationToken = default);
    Task Delete(int id, CancellationToken cancellationToken = default);
    Task UpdateOrderOfItems(IEnumerable<FooterMenuItemDTO> items, CancellationToken cancellationToken = default);
}
