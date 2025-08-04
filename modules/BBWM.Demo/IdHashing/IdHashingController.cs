using BBWM.Core.Filters;
using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.ModelBinders;
using BBWM.Demo.IdHashing.DTO;
using BBWM.Demo.Northwind.Model;

using Microsoft.AspNetCore.Mvc;

namespace BBWM.Demo.IdHashing;

[Route("api/demo/id-hashing")]
public class IdHashingController : DataControllerBase<IDemoDataContext, Order, OrderHashingDTO, OrderHashingDTO, int>
{
    private readonly IDataService<IDemoDataContext> dataService;
    private readonly IOrderHashingService orderHashingService;

    public IdHashingController(IDataService<IDemoDataContext> dataService, IOrderHashingService orderHashingService)
        : base(dataService, orderHashingService)
    {
        this.dataService = dataService;
        this.orderHashingService = orderHashingService;
    }

    [HttpGet("simple/page")]
    public async Task<IActionResult> GetSimpleOrdersPageAsync(
        [FromQuery] QueryCommand command, CancellationToken ct = default)
        => Ok(await dataService.GetPage<Order, SimpleOrderHashingDTO>(command, orderHashingService, ct));

    [HttpGet("info/{id}")]
    public async Task<IActionResult> GetInfo([HashedKeyBinder] int id, CancellationToken ct)
        => Ok(await dataService.Get<Order, OrderHashingDTO>(id, orderHashingService, ct));
}
