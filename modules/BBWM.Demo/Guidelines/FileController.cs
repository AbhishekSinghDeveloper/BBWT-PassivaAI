using BBWM.Core.Filters;
using BBWM.Core.Services;
using BBWM.Core.Web;

using Microsoft.AspNetCore.Mvc;

namespace BBWM.Demo.Guidelines;

/// <summary>
/// Controller to provide functionality for files
/// </summary>
[Route("api/demo/file")]
public class FileController : DataControllerBase<IDemoDataContext, File, FileDTO, FileDTO, int>
{
    public FileController(IDataService<IDemoDataContext> service, IFileService fileService) : base(service, fileService)
    {
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] Filter filter, CancellationToken ct = default) =>
        Ok(await DataService.GetAll<File, FileDTO>(filter, ct));
}
