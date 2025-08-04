using BBWM.Core.Services;

namespace BBWM.StaticPages;

public interface IStaticPageService : IEntityCreate<StaticPageDTO>, IEntityUpdate<StaticPageDTO>
{
    Task<bool> CheckExist(StaticPageDTO page, CancellationToken ct = default);
    Task<StaticPageDTO> GetByUrl(string url, CancellationToken ct = default);
}
