using BBWM.Core.Filters;
using BBWM.Core.Services;
using BBWM.Demo.Northwind.DTO;
using BBWM.Demo.Northwind.Model;
using BBWM.Demo.Northwind.Services;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using ControllerBase = BBWM.Core.Web.ControllerBase;

namespace BBWM.Demo.Security;

[Route("api/demo/security-test")]
public class SecurityTestController : ControllerBase
{
    private readonly IOrderService _orderService;
    private readonly IDataService<IDemoDataContext> dataService;
    private readonly IAuthorizationService _authorizationService;

    public SecurityTestController(
        IOrderService orderService,
        IDataService<IDemoDataContext> dataService,
        IAuthorizationService authorizationService)
    {
        _orderService = orderService;
        this.dataService = dataService;
        _authorizationService = authorizationService;
    }


    // Note: Authorize filter automatically applies to this method due to the fact that it defined in the Startup.cs
    [HttpGet, Route("accessible/authenticated")]
    public async Task<IActionResult> Authenticated(QueryCommand query, CancellationToken cancellationToken)
    {
        var page = await _orderService.GetPage(query, cancellationToken);
        foreach (var prder in page.Items)
            prder.CustomerCode += $" - Authenticated User";

        return Ok(page);
    }

    [HttpGet, Route("accessible/group/{groupName}")]
    [Authorize(Permissions.AccessControlDemoViewOrders)]
    public async Task<IActionResult> GroupGetByGroupName(string groupName, CancellationToken cancellationToken)
    {
        var order = await (groupName == Groups.GroupA
            ? dataService.Get<Order, OrderDTO>(q => q.Where(order => order.OrderDate.HasValue))
            : dataService.Get<Order, OrderDTO>(q => q.Where(order => order.ShippedDate.HasValue)));

        if (order is null)
            return Ok();

        var authRes = await _authorizationService.AuthorizeAsync(
            User,
            new AccessibleToGroupForIdResourceInfo(order),
            new AccessibleToGroupRequirement());

        return authRes.Succeeded ? Ok(order) : Forbid();
    }

    [HttpGet, Route("accessible/group")]
    [Authorize(Permissions.AccessControlDemoViewOrders)]
    public async Task<IActionResult> GroupGetPage(QueryCommand query, CancellationToken cancellationToken)
    {
        //TODO: commented _carService usage due to CRUD (2021) changes. We need to fully review
        // this groups access aprooach. Dependency on IDataService seems like overcomplication
        var authRes = await _authorizationService.AuthorizeAsync(User,
            new AccessibleToGroupForListResourceInfo(null /*_carService*/), new AccessibleToGroupRequirement());

        return authRes.Succeeded
            ? Ok(await _orderService.GetPage(query, cancellationToken))
            : Forbid();
    }

    [Authorize(Permissions.AccessControlDemoViewOrders)]
    [Authorize(Policies.BelongsToGroup)]
    public IActionResult PostTest([FromBody] object data = null) => Ok(data);
}
