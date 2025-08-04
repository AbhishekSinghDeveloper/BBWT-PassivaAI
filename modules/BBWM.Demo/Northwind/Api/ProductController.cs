using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Demo.Northwind.DTO;
using BBWM.Demo.Northwind.Model;

using Microsoft.AspNetCore.Mvc;

namespace BBWM.Demo.Northwind.Api;

[Route("api/demo/product")]
public class ProductController : DataControllerBase<IDemoDataContext, Product, ProductDTO, ProductDTO, int>
{
    public ProductController(IDataService<IDemoDataContext> dataService) : base(dataService)
    {
    }

    [HttpDelete]
    public Task<IActionResult> DeleteAll(CancellationToken cancellationToken = default)
        => NoContent(async () => await DataService.DeleteAll<Product>(cancellationToken));
}
