using BBWM.Menu.DTO;

namespace BBWM.Menu;

public interface IFooterMenuDataProvider
{
    Task<bool> Exists(int id, CancellationToken cancellationToken = default);

    Task<List<FooterMenuItemDTO>> GetAll(CancellationToken cancellationToken = default);

    Task<FooterMenuItemDTO> Get(int id, CancellationToken cancellationToken = default);

    Task<FooterMenuItemDTO> GetByLink(string routerLink, CancellationToken cancellationToken = default);

    Task<IEnumerable<FooterMenuItemDTO>> AddRange(IEnumerable<FooterMenuItemDTO> menu, CancellationToken cancellationToken = default);

    Task<IEnumerable<FooterMenuItemDTO>> UpdateRange(IEnumerable<FooterMenuItemDTO> menu, CancellationToken cancellationToken = default);

    Task<FooterMenuItemDTO> Save(FooterMenuItemDTO menu, CancellationToken cancellationToken = default);

    Task Delete(int id, CancellationToken cancellationToken = default);
}
