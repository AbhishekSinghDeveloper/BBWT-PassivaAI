using BBWM.Core.Filters;
using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Demo.Northwind.DTO;
using BBWM.Demo.Northwind.Model;
using BBWM.Demo.Northwind.Services;

using Microsoft.AspNetCore.Mvc;

namespace BBWM.Demo.Northwind.Api;

[Route("api/demo/customer")]
public class CustomerController : DataControllerBase<IDemoDataContext, Customer, CustomerDTO, CustomerDTO, int>
{
    private readonly ICustomerService _customerService;

    public CustomerController(IDataService<IDemoDataContext> dataService, ICustomerService customerService) : base(dataService, customerService) =>
        _customerService = customerService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(await DataService.GetAll<Customer, CustomerDTO>(ct));

    [HttpDelete]
    public Task<IActionResult> DeleteAll(CancellationToken ct = default)
        => NoContent(async () => await DataService.DeleteAll<Customer>(ct));

    [HttpGet("search")]
    public async Task<IActionResult> GetListForSearch([FromQuery] QueryCommand command, CancellationToken ct = default)
        => Ok(await DataService.GetPage<Customer, SearchCustomerDTO>(command, ct));

    [HttpGet("all-companies")]
    public IActionResult GetAllCompanies() => Ok(_customerService.GetAllCompanies());
}
