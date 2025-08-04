using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.Filters;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using CoreRoles = BBWM.Core.Roles;

namespace BBWT.Server.Api;

[Produces("application/json")]
[Route("api/branding")]
[ReadWriteAuthorize(ReadWriteRoles = CoreRoles.SystemAdminRole)]
public class BrandingController : DataControllerBase<Branding, BrandingDTO, BrandingDTO>
{
    private readonly IBrandingService _brandingService;
    private readonly UserManager<User> _userManager;
    private readonly IHttpContextAccessor _contextAccessor;


    public BrandingController(
        IDataService dataService,
        IBrandingService brandingService,
        UserManager<User> userManager,
        IHttpContextAccessor contextAccessor)
        : base(dataService, brandingService)
    {
        _brandingService = brandingService;
        _userManager = userManager;
        _contextAccessor = contextAccessor;
    }


    [HttpGet, Route("me")]
    [Authorize]
    public async Task<IActionResult> GetBrandForCurrentUser(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(_contextAccessor.HttpContext.User);

        var branding = user.OrganizationId is not null ?
            await _brandingService.GetOrganizationBranding(user.OrganizationId.Value, cancellationToken) : null;

        return branding is null ? NoContent() : Ok(branding);
    }
}
