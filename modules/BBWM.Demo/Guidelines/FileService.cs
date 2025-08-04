using BBWM.Core.Services;

using Microsoft.EntityFrameworkCore;

namespace BBWM.Demo.Guidelines;

public interface IFileService : IEntityDelete<int>, IEntityQuery<FileDTO> { }

public class FileService : IFileService
{
    private readonly IDataService<IDemoDataContext> _dataService;

    public FileService(IDataService<IDemoDataContext> dataService)
        => _dataService = dataService;

    public async Task Delete(int id, CancellationToken ct = default)
    {
        var file = await _dataService.Get<File, FileDTO>(id, query => query.Include(o => o.Children), ct);
        if (file is null) return;

        if (file.Children.Any())
        {
            foreach (var child in file.Children)
            {
                await Delete(child.Id, ct);
            }
        }

        await _dataService.Delete<File>(id);
    }

    public IQueryable<FileDTO> GetEntityQuery(IQueryable<FileDTO> baseQuery)
        => baseQuery.Include(x => x.Parent).Include(x => x.Children);
}
