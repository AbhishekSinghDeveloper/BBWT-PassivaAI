using BBWM.Core.Exceptions;
using BBWM.Menu.DTO;

namespace BBWM.Menu;

public class FooterMenuService : IFooterMenuService
{
    private readonly IFooterMenuDataProvider _dataProvider;


    public FooterMenuService(IFooterMenuDataProvider dataProvider) =>
        _dataProvider = dataProvider;


    public async Task<bool> Exists(int id, CancellationToken cancellationToken = default) =>
        await _dataProvider.Exists(id, cancellationToken);

    public async Task<List<FooterMenuItemDTO>> GetAllAsync(CancellationToken cancellationToken = default) =>
        (await _dataProvider.GetAll(cancellationToken)).OrderBy(x => x.OrderNo).ToList();

    public async Task UpdateOrderOfItems(IEnumerable<FooterMenuItemDTO> items, CancellationToken cancellationToken = default) =>
        await _dataProvider.UpdateRange(items, cancellationToken);

    public async Task<FooterMenuItemDTO> Save(FooterMenuItemDTO item, CancellationToken cancellationToken = default)
    {
        var existingMenuItem = await _dataProvider.GetByLink(item.RouterLink, cancellationToken);
        if (existingMenuItem is not null && existingMenuItem.Id != item.Id)
            throw new BusinessException("Menu item with specified link already exists.");

        if (item.OrderNo == default)
            item.OrderNo = (await GetAllAsync(cancellationToken)).Select(i => i.OrderNo).DefaultIfEmpty().Max() + 1;
        return await _dataProvider.Save(item, cancellationToken);
    }

    public async Task Delete(int id, CancellationToken cancellationToken = default) =>
        await _dataProvider.Delete(id, cancellationToken);
}
