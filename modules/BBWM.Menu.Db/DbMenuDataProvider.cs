using AutoMapper;

using BBWM.Core.Data;
using BBWM.Menu.DTO;

using Microsoft.EntityFrameworkCore;

namespace BBWM.Menu.Db;

public class DbMenuDataProvider : IMenuDataProvider
{
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public DbMenuDataProvider(IDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<MenuDTO>> GetAll(CancellationToken cancellationToken = default)
    {
        return _mapper.Map<List<MenuDTO>>(await _context.Set<MenuItem>().Include(i => i.Parent).ToListAsync(cancellationToken));
    }

    public async Task<bool> UpdateRange(IEnumerable<MenuDTO> menu, CancellationToken cancellationToken = default)
    {
        var ids = menu.Select(i => i.Id).ToArray();
        var entities = await _context.Set<MenuItem>().Where(i => ids.Contains(i.Id)).ToArrayAsync(cancellationToken);
        foreach (var item in menu)
        {
            var entity = entities.FirstOrDefault(i => i.Id == item.Id);
            if (entity is not null)
            {
                entity.RouterLink = item.RouterLink;
                entity.RouterLink = entity.RouterLink == "" ? null : entity.RouterLink;
                entity.Label = item.Label;
                entity.Icon = item.Icon;
                entity.Hidden = item.Hidden;
                entity.ParentId = item.ParentId;
                entity.Index = item.Index;
            }
        }

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<int> Create(MenuDTO menu, CancellationToken cancellationToken = default)
    {
        var entity = _mapper.Map<MenuItem>(menu);
        _context.Set<MenuItem>().Add(entity);
        await _context.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }

    public async Task AddRange(IEnumerable<MenuDTO> items, CancellationToken cancellationToken = default)
    {
        var entities = _mapper.Map<List<MenuItem>>(items);
        _context.Set<MenuItem>().AddRange(entities);
        await _context.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> Delete(int id, CancellationToken cancellationToken = default)
    {
        var entity = _context.Set<MenuItem>().Include(x => x.Parent).ToList().FirstOrDefault(c => c.Id == id);

        if (entity is null) return false;

        if (entity.Children is null) entity.Children = new List<MenuItem>();

        while (entity.Children.Count > 0)
        {
            DeleteChildren(entity.Children);
            await _context.SaveChangesAsync(cancellationToken);
        }
        _context.Set<MenuItem>().Remove(entity);
        await _context.SaveChangesAsync(cancellationToken);

        return true;
    }

    public int GetMaxIndex(int? parentId = null)
    {
        var indexes = _context.Set<MenuItem>().Where(x => x.ParentId == parentId).Select(x => x.Index);
        return indexes.Any() ? indexes.Max() : -1;
    }

    private void DeleteChildren(ICollection<MenuItem> children)
    {
        foreach (var child in children)
        {
            if (child.Children is null) child.Children = new List<MenuItem>();

            if (child.Children.Count > 0)
            {
                DeleteChildren(child.Children);
            }
            else
            {
                _context.Set<MenuItem>().Remove(child);
            }
        }
    }
}
