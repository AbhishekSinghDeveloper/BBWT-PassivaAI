using BBWM.AWS;
using BBWM.Core.Filters;
using BBWM.FileStorage;

using Microsoft.AspNetCore.Mvc;

using ControllerBase = BBWM.Core.Web.ControllerBase;

namespace BBWM.Demo.S3FileManager;

[Produces("application/json")]
[Route("api/demo/s3-file-manager")]
public class AwsStorageController : ControllerBase
{
    private readonly IAwsService _awsService;


    public AwsStorageController(IAwsService awsService)
        => _awsService = awsService;


    [HttpGet, Route("regions")]
    public IActionResult GetRegions() =>
        Ok(_awsService.GetRegions());

    [HttpGet, Route("presigned/{key}")]
    public IActionResult GetPreSignedUrl(string key) =>
        Ok(_awsService.GeneratePreSignedURL(key));

    [HttpGet, Route("images")]
    public async Task<IActionResult> GetAllImages(CancellationToken cancellationToken) =>
        Ok(await _awsService.GetAllImages(cancellationToken));

    [HttpGet, Route("files")]
    public async Task<IActionResult> GetAllFiles(CancellationToken cancellationToken) =>
        Ok(await _awsService.GetAllFiles(cancellationToken));

    [HttpGet, Route("page")]
    public async Task<IActionResult> GetPage(QueryCommand command, CancellationToken cancellationToken = default)
    {
        var files = await _awsService.GetAllFiles(cancellationToken);

        var total = files.Count;

        if (command.Skip is not null)
            files = files.Skip(command.Skip.Value).ToList();
        if (command.Take is not null)
            files = files.Take(command.Take.Value).ToList();

        return Ok(new PageResult<StorageFileData>
        {
            Items = files,
            Total = total
        });
    }
}
