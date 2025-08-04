using BBWM.Core.Services;
using BBWM.Demo.Northwind.DTO;
using BBWM.Demo.Northwind.Model;

using Microsoft.AspNetCore.Mvc;

namespace BBWM.Demo.Culture;

[Route("api/demo/culture")]
public class CultureController : Controller
{
    [HttpPost]
    public async Task<TestTimezoneDTO> Post([FromBody] DateTime clientDate,
        [FromServices] IDataService<IDemoDataContext> dataService,
        CancellationToken cancellationToken = default)
    {
        var order = await dataService.Create<Order, OrderDTO>(
            new OrderDTO { OrderDate = clientDate }, cancellationToken);

        var serverDateResult = await dataService.Get<Order, OrderDTO>(order.Id, cancellationToken);

        if (serverDateResult.OrderDate is null) return null;

        var result = new TestTimezoneDTO
        {
            ClientDate = clientDate,
            ServerDate = serverDateResult.OrderDate.Value
        };

        await dataService.Delete<Order>(order.Id, cancellationToken);
        return result;
    }
}
