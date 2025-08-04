using BBWM.Core.Services;

namespace BBWM.StaticPages;

public class StaticPageService : IStaticPageService
{
    private readonly IDataService _dataService;

    public StaticPageService(IDataService dataService)
    {
        _dataService = dataService;
    }

    public Task<StaticPageDTO> Create(StaticPageDTO dto, CancellationToken ct = default)
        => _dataService.Create<StaticPage, StaticPageDTO>(BeforeSave(dto), ct);

    public Task<StaticPageDTO> Update(StaticPageDTO dto, CancellationToken ct = default)
        => _dataService.Update<StaticPage, StaticPageDTO>(BeforeSave(dto), ct);

    private static StaticPageDTO BeforeSave(StaticPageDTO dto)
    {
        // we should use a service to provide DateTime.Now to make the code more testable
        var currentDateTime = DateTime.Now;
        dto.LastUpdated = currentDateTime.AddTicks(-(currentDateTime.Ticks % TimeSpan.TicksPerSecond));
        return dto;
    }

    public Task<bool> CheckExist(StaticPageDTO page, CancellationToken ct = default) =>
        _dataService.Any<StaticPage>(query => query.Where(x => x.Alias.Equals(page.Alias) && x.Id != page.Id), ct);

    public async Task<StaticPageDTO> GetByUrl(string url, CancellationToken ct = default)
    {
        var urlAlias = url.Replace("/app/static/", "");
        return await _dataService.Get<StaticPage, StaticPageDTO>(query => query.Where(x => x.Alias.Equals(urlAlias)), ct);
    }
}
