using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;
using BBWM.Core.Web;

using Microsoft.AspNetCore.Mvc;

namespace BBWM.Demo.Security;

[Route("api/demo/group")]
public class GroupController : DataControllerBase<Group, GroupDTO, GroupDTO>
{
    public GroupController(IDataService dataService) : base(dataService)
    {
    }
}
