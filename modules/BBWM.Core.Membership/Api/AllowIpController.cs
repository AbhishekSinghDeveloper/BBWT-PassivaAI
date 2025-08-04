using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Services;
using BBWM.Core.Services;
using BBWM.Core.Web;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.Core.Membership.Api;

[Route("api/allowed-ip")]
[Authorize(Roles = Core.Roles.SystemAdminRole + "," + Core.Roles.SuperAdminRole)]
public class AllowIpController : DataControllerBase<AllowedIp, AllowedIpDTO, AllowedIpDTO>
{
    public AllowIpController(
        IDataService dataService,
        IAllowedIpDataService allowIpService)
        : base(dataService, allowIpService)
    {
    }
}
