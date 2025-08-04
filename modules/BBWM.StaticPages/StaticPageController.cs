using BBWM.Core.Filters;
using BBWM.Core.ModelHashing;
using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.Filters;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using System.ComponentModel.DataAnnotations;

namespace BBWM.StaticPages;

[Route("api/static-page")]
[ReadWriteAuthorize(ReadWriteRoles = Core.Roles.SystemAdminRole)]
public class StaticPageController : DataControllerBase<StaticPage, StaticPageDTO, StaticPageDTO>
{
    private readonly IStaticPageService _staticPageService;

    public StaticPageController(IDataService dataService, IStaticPageService staticPageService)
        : base(dataService, staticPageService)
        => _staticPageService = staticPageService;

    /// <summary>
    /// Gets a full list of static pages.
    /// Static pages are edited by a user with the SystemAdmin role. Additionally the SuperAdmin role is allowed
    /// to access GetAll() method because SuperAdmin needs this list on the Menu Configuration page.
    /// </summary>
    [Authorize(Roles = Core.Roles.SystemAdminRole + "," + Core.Roles.SuperAdminRole)]
    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Filter filter, CancellationToken cancellationToken = default) =>
        Ok(await DataService.GetAll<StaticPage, StaticPageDTO>(cancellationToken));

    public override async Task<IActionResult> Create([FromBody] StaticPageDTO dto, [FromServices] IModelHashingService modelHashingService, CancellationToken cancellationToken = default) =>
        await _staticPageService.CheckExist(dto, cancellationToken)
            ? BadRequest("A static page with this alias already exists.")
            : await base.Create(dto, modelHashingService, cancellationToken);

    public override async Task<IActionResult> Update([FromBody] StaticPageDTO dto, CancellationToken cancellationToken = default) =>
        await _staticPageService.CheckExist(dto, cancellationToken)
            ? BadRequest("A static page with this alias already exists.")
            : await base.Update(dto, cancellationToken);

    [HttpGet, Route("by-url")]
    [Authorize]
    public async Task<IActionResult> GetByUrl([Required] string url, CancellationToken cancellationToken) =>
        Ok(await _staticPageService.GetByUrl(url, cancellationToken));
}
