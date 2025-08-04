using AutoMapper;
using BBWM.Core.Membership.Interfaces;
using BBWM.Menu.DTO;

namespace BBWM.Menu;

public class MenuService : IMenuService
{
    private readonly IMenuDataProvider _dataProvider;
    private readonly IMapper _mapper;
    private readonly IRouteRolesService _routesService;


    public MenuService(IMenuDataProvider dataProvider, IMapper mapper, IRouteRolesService routeRolesService)
    {
        _dataProvider = dataProvider;
        _mapper = mapper;
        _routesService = routeRolesService;
    }


    public async Task<List<MenuDTO>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var menu = new List<MenuDTO>();
        var result = await _dataProvider.GetAll(cancellationToken);

        foreach (var item in result)
        {
            if (item.ParentId is null || item.ParentId == default(int))
            {
                menu.Add(item);
            }
        }
        menu = menu.OrderBy(x => x.Index).ToList();

        foreach (var item in menu)
        {
            var itemChildren = item.Children?.ToArray() ?? new MenuDTO[] { };
            if (itemChildren.Any())
            {
                item.Children = SortMenuItems(itemChildren);
            }
        }

        return menu;
    }

    public async Task<List<MenuDTO>> GetForUser(string userId, CancellationToken cancellationToken = default) =>
        FilterOnlyAllowedMenuItems(await GetAllAsync(cancellationToken), await _routesService.GetPageRoutesForUser(userId, cancellationToken));

    public async Task<bool> Delete(int id, CancellationToken cancellationToken)
    {
        return await _dataProvider.Delete(id, cancellationToken);
    }

    public async Task<int> Update(MenuDTO item, CancellationToken cancellationToken)
    {
        var menuItems = await _dataProvider.GetAll(cancellationToken);
        var entity = menuItems.FirstOrDefault(i => i.Id == item.Id);
        if (entity is null) return 0;

        var itemsToUpdate = new List<MenuDTO>();
        // Save the ne position and set index to items in order to organise the group
        if (entity.ParentId != item.ParentId)
        {

            // Set Index to previous item group
            var oldParentChildren = menuItems.Where(x => x.Index != entity.Index && x.ParentId == entity.ParentId).OrderBy(o => o.Index).ToList();
            var oldi = 0;
            foreach (var child in oldParentChildren)
            {
                child.Index = oldi;
                oldi++;
            }

            // Set Index to new item group
            var newParentChildren = menuItems.Where(x => x.Index >= item.Index && x.ParentId == item.ParentId).ToList();
            foreach (var child in newParentChildren)
            {
                child.Index++;
            }

            itemsToUpdate = oldParentChildren.Concat(newParentChildren).ToList();
        }
        else if (entity.Index != item.Index)
        {
            if (entity.Index < item.Index)
            {
                var parentChildren = menuItems.Where(x => x.Index > entity.Index && x.Index <= item.Index && x.ParentId == entity.ParentId).ToList();
                foreach (var child in parentChildren)
                {
                    child.Index--;
                }
                itemsToUpdate = parentChildren;
            }
            else if (entity.Index > item.Index)
            {
                var parentChildren = menuItems.Where(x => x.Index >= item.Index && x.Index < entity.Index && x.ParentId == entity.ParentId).ToList();
                foreach (var child in parentChildren)
                {
                    child.Index++;
                }
                itemsToUpdate = parentChildren;
            }
        }

        itemsToUpdate.Add(item);
        await _dataProvider.UpdateRange(itemsToUpdate, cancellationToken);
        return entity.Id;
    }

    public async Task<int> Create(MenuDTO item, CancellationToken cancellationToken)
    {
        item.RouterLink = item.RouterLink == string.Empty ? null : item.RouterLink;

        item.Index = _dataProvider.GetMaxIndex(item.ParentId) + 1;

        return await _dataProvider.Create(item, cancellationToken);
    }


    private ICollection<MenuDTO> SortMenuItems(ICollection<MenuDTO> items)
    {
        var menuDtos = items;
        foreach (var item in menuDtos)
        {
            var itemChildren = item.Children?.ToArray() ?? new MenuDTO[] { };
            if (itemChildren.Any())
            {
                item.Children = SortMenuItems(itemChildren);
            }
        }
        return menuDtos.OrderBy(x => x.Index).ToList();
    }

    private List<MenuDTO> FilterOnlyAllowedMenuItems(List<MenuDTO> items, string[] allowedRoutes)
    {
        var result = new List<MenuDTO>();

        foreach (var item in items)
        {
            if (string.IsNullOrEmpty(item.RouterLink) || allowedRoutes.Any(x => x == item.RouterLink))
            {
                var newItem = new MenuDTO
                {
                    Id = item.Id,
                    Index = item.Index,
                    Href = item.Href,
                    ParentId = item.ParentId,
                    RouterLink = item.RouterLink,
                    Label = item.Label,
                    Icon = item.Icon,
                    Hidden = item.Hidden,
                    Disabled = item.Disabled,
                    Classes = item.Classes,
                    CustomHandler = item.CustomHandler,
                    Children = new List<MenuDTO>()
                };

                var isParentMenu = item.Children is not null && item.Children.Count > 0;
                if (isParentMenu)
                {
                    newItem.Children = FilterOnlyAllowedMenuItems(item.Children.ToList(), allowedRoutes);
                }

                if (!isParentMenu || newItem.Children.Count > 0)
                {
                    result.Add(newItem);
                }
            }
        }

        return result;
    }
}
