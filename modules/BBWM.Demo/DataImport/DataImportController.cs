using BBWM.Core.Autofac;
using BBWM.DataProcessing.Classes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using System.Net;
using System.Security.Claims;
using U2F.Core.Utils;

namespace BBWM.Demo.DataImport;

[Authorize]
[Route("api/demo/data-import")]
public class DataImportController : Controller
{
    private readonly string csvSamplePath;

    public DataImportController(IHostEnvironment environment) =>
        csvSamplePath = $"{environment.ContentRootPath}/data/demo/data-import/";

    [HttpPost, Route("[action]"), IgnoreLogging]
    public async Task<IActionResult> Import(ImportDataModel model,
        [FromServices] IEmployeesImportService employeesImportService)
    {
        try
        {
            var memoryStream = new MemoryStream();
            await model.File.CopyToAsync(memoryStream);

            return Ok(await employeesImportService.Import(
                employeesImportService.CreateSettings(model, memoryStream,
                    User.FindFirstValue(ClaimTypes.NameIdentifier)), default));
        }
        catch (Exception ex)
        {
            return StatusCode((int)HttpStatusCode.Forbidden, ex.Message);
        }
    }

    [HttpGet("csv-sample/{fileName}"), ResponseCache(NoStore = true)]
    public FileContentResult GetCsvFileSampleValid(string fileName) =>
        File(System.IO.File.ReadAllText(csvSamplePath + fileName).GetBytes(), "text/csv", fileName);
}
