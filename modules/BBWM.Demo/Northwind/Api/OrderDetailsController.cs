using BBWM.Core.Filters;
using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Demo.Northwind.DTO;
using BBWM.Demo.Northwind.Model;

using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BBWM.Demo.Northwind.Api;

[Route("api/demo/order-details")]
public class OrderDetailsController : DataControllerBase<IDemoDataContext, OrderDetails, OrderDetailsDTO, OrderDetailsDTO, int>
{
    public OrderDetailsController(IDataService<IDemoDataContext> service) : base(service)
    {
    }

    [HttpGet, Route("page")]
    public override async Task<IActionResult> GetPage([FromQuery] QueryCommand command, CancellationToken ct = default)
        => Ok(await DataService.GetPage<OrderDetails, OrderDetailsDTO>(command,
            query => query.Include(l => l.Product),
            ct));

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Filter filter, CancellationToken cancellationToken = default) =>
        Ok(await DataService.GetAll<OrderDetails, OrderDetailsDTO>(cancellationToken));

    [HttpDelete]
    public Task<IActionResult> DeleteAll(CancellationToken cancellationToken = default)
        => NoContent(async () => await DataService.DeleteAll<OrderDetails>(cancellationToken));
}
