using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.DbDoc.Web;
using BBWM.Demo.Northwind.DTO;
using BBWM.Demo.Northwind.Model;
using BBWM.Demo.Northwind.Services;

using Microsoft.AspNetCore.Mvc;

namespace BBWM.Demo.Northwind.Api;

[Route("api/demo/order")]
[DbDocMetadataValidationFilter]
public class OrderController : DataControllerBase<IDemoDataContext, Order, OrderDTO, OrderDTO, int>
{
    public OrderController(IDataService<IDemoDataContext> dataService, IOrderService service)
        : base(dataService, service)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken = default)
        // NOTE! Getting all elements/rows from a table is usually not a good idea and should be
        //       carefully analyzed. As this is for demonstration purposes only we intentionally
        //       take a small number of elements to return to the client as there can potentially
        //       be thousands of records.
        => Ok(await DataService.GetAll<Order, OrderDTO>(q => q.Take(100), cancellationToken));

    [HttpDelete]
    public Task<IActionResult> DeleteAll(CancellationToken cancellationToken = default)
        => NoContent(async () => await DataService.DeleteAll<Order>(cancellationToken));
}
