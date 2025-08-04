using BBWM.FileStorage;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

using ControllerBase = BBWM.Core.Web.ControllerBase;

namespace BBWM.Demo.Controllers;

[Produces("application/json")]
[Route("api/demo/file-storage")]
public class FileStorageController : ControllerBase
{
    private const string UploadingOperationName = "DemoImageUploading";

    private readonly IFileStorageService _fileStorageService;


    public FileStorageController(IFileStorageService fileStorageService)
        => _fileStorageService = fileStorageService;


    [HttpGet, Route("images")]
    public async Task<IActionResult> GetAllImages(CancellationToken cancellationToken = default) =>
        Ok(await _fileStorageService.GetAllImages(UploadingOperationName, cancellationToken));

    [HttpGet, Route("files")]
    public async Task<IActionResult> GetAllFiles(CancellationToken cancellationToken = default) =>
        Ok(await _fileStorageService.GetAllFiles(UploadingOperationName, cancellationToken));

    [HttpPost]
    public async Task<IActionResult> UploadImages(IFormCollection formData, CancellationToken cancellationToken = default)
    {
        var additionalData = formData.Keys.ToDictionary<string, string, string>(key => key, key => formData[key]);
        additionalData.Add("operation_name", UploadingOperationName);

        return Ok(await _fileStorageService.UploadFiles(Request.Form.Files.ToArray(), additionalData, cancellationToken));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteFile(string key, CancellationToken cancellationToken = default) =>
        Ok(await _fileStorageService.DeleteFile(key, cancellationToken));
}
