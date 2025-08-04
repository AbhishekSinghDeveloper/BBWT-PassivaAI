using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Demo.Northwind.DTO;
using BBWM.Demo.Northwind.Model;

using Microsoft.AspNetCore.Mvc;

namespace BBWM.Demo.Northwind.Api;

/// <summary>
/// Controller to provide functionality for employees
/// </summary>
[Route("api/demo/employee")]
public class EmployeeController : DataControllerBase<IDemoDataContext, Employee, EmployeeDTO, EmployeeDTO, int>
{
    /// <summary>
    /// Constructs the controller
    /// </summary>
    /// <param name="employeeService">Employees service</param>
    public EmployeeController(IDataService<IDemoDataContext> dataService) : base(dataService)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct = default)
        => Ok(await DataService.GetAll<Employee, EmployeeDTO>(ct));

    [HttpDelete]
    public Task<IActionResult> DeleteAll(CancellationToken cancellationToken = default)
        => NoContent(async () => await DataService.DeleteAll<Employee>(cancellationToken));
}
