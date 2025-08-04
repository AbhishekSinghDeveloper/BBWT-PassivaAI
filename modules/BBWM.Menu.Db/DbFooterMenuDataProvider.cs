using AutoMapper;

using BBWM.Core.Data;
using BBWM.Core.Exceptions;
using BBWM.Menu.DTO;

using Microsoft.EntityFrameworkCore;

namespace BBWM.Menu.Db;

public class DbFooterMenuDataProvider : IFooterMenuDataProvider
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;


    public DbFooterMenuDataProvider(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }


    public async Task<bool> Exists(int id, CancellationToken cancellationToken = default) =>
        await _context.Set<FooterMenuItem>().AnyAsync(x => x.Id == id, cancellationToken);

    public async Task<List<FooterMenuItemDTO>> GetAll(CancellationToken cancellationToken = default) =>
        _mapper.Map<List<FooterMenuItemDTO>>(await _context.Set<FooterMenuItem>().OrderBy(i => i.OrderNo).ToListAsync(cancellationToken));

    public async Task<FooterMenuItemDTO> Get(int id, CancellationToken cancellationToken = default) =>
        _mapper.Map<FooterMenuItemDTO>(await _context.Set<FooterMenuItem>()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken));

    public async Task<FooterMenuItemDTO> GetByLink(string routerLink, CancellationToken cancellationToken = default) =>
        _mapper.Map<FooterMenuItemDTO>(await _context.Set<FooterMenuItem>().FirstOrDefaultAsync(
            i => i.RouterLink.Equals(routerLink, StringComparison.OrdinalIgnoreCase),
            cancellationToken));

    public async Task<IEnumerable<FooterMenuItemDTO>> AddRange(IEnumerable<FooterMenuItemDTO> menu, CancellationToken cancellationToken = default)
    {
        var entities = _mapper.Map<List<FooterMenuItem>>(menu);
        _context.Set<FooterMenuItem>().AddRange(entities);
        await _context.SaveChangesAsync(cancellationToken);
        return menu;
    }

    public async Task<IEnumerable<FooterMenuItemDTO>> UpdateRange(IEnumerable<FooterMenuItemDTO> menu, CancellationToken cancellationToken = default)
    {
        var footerMenuItems = menu as FooterMenuItemDTO[] ?? menu.ToArray();
        var ids = footerMenuItems.Select(i => i.Id).ToArray();
        var entities = await _context.Set<FooterMenuItem>().Where(i => ids.Contains(i.Id)).ToArrayAsync(cancellationToken);
        var result = new List<FooterMenuItem>();
        foreach (var item in footerMenuItems)
        {
            var entity = entities.FirstOrDefault(i => i.Id == item.Id);
            if (entity is not null)
            {
                entity.RouterLink = item.RouterLink;
                entity.RouterLink = entity.RouterLink == "" ? null : entity.RouterLink;
                entity.OrderNo = item.OrderNo;
                entity.Name = item.Name;

                result.Add(entity);
            }
        }

        await _context.SaveChangesAsync(cancellationToken);

        return _mapper.Map<IEnumerable<FooterMenuItemDTO>>(result);
    }

    public async Task<FooterMenuItemDTO> Save(FooterMenuItemDTO menu, CancellationToken cancellationToken = default)
    {
        var entity = menu.Id > 0 ? _context.Set<FooterMenuItem>().Find(menu.Id) : new FooterMenuItem();
        _mapper.Map(menu, entity);
        if (menu.Id == 0)
            _context.Set<FooterMenuItem>().Add(entity);

        await _context.SaveChangesAsync(cancellationToken);
        return await Get(entity.Id, cancellationToken);
    }

    public async Task Delete(int id, CancellationToken cancellationToken = default)
    {
        var entity = _context.Set<FooterMenuItem>().FirstOrDefault(x => x.Id == id);
        if (entity is null)
            throw new ObjectNotExistsException("Footer menu item doesn't exist.");

        _context.Set<FooterMenuItem>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);
    }
}
