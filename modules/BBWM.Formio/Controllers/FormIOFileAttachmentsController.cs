using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using BBWM.Core.Data;
using BBWM.Core.Web.ModelBinders;
using BBWM.FileStorage;
using BBWM.FormIO.DTO;
using BBWM.FormIO.Interfaces;

namespace BBWM.FormIO.Controllers;

[Produces("application/json")]
[Route("api/file-attachments")]
public class FormIOFileAttachmentsController : ControllerBase
{
    private const string UploadingOperationName = "DemoFileAttachment";
    private const string UploadingOperationNameImage = "FormIOImageUploading";

    private readonly IFileStorageService _fileStorageService;
    private readonly IFormIOFileService _fileService;
    private readonly IDbContext _context;
    private readonly IMapper _mapper;

    public FormIOFileAttachmentsController(IFileStorageService fileStorageService, IFormIOFileService fileService, IDbContext context, IMapper mapper)
    {
        _fileStorageService = fileStorageService;
        _fileService = fileService;
        _mapper = mapper;
        _context = context;
    }

    [HttpGet, Route("{key}")]
    public async Task<IActionResult> Get(string key, CancellationToken ct = default) =>
        Ok(await _fileService.GetFile(key, ct));

    [HttpGet, Route("images")]
    public async Task<IActionResult> GetAllImages(CancellationToken ct = default) =>
        Ok(await _fileStorageService.GetAllImages(UploadingOperationNameImage, ct));

    [HttpGet, Route("files")]
    public async Task<IActionResult> GetAllFiles(CancellationToken ct = default) =>
        Ok(await _fileStorageService.GetAllFiles(UploadingOperationNameImage, ct));

    [HttpGet, Route("image/{key}")]
    public async Task<IActionResult> GetSingleImage(string key, CancellationToken ct = default)
    {
        var result = _mapper.Map<FileDetailsDTO>(await _context.Set<FileDetails>()
            .FirstOrDefaultAsync(x => x.Key == key.Split(".", StringSplitOptions.None).First(), ct));
        return Ok(result);
    }

    [HttpPost]
    [Route("file")]
    public async Task<IActionResult> UploadFiles(IFormCollection formData, CancellationToken ct = default)
    {
        var additionalData = formData.Keys.ToDictionary<string, string, string>(key => key, key => formData[key]);
        additionalData.Add("operation_name", UploadingOperationName);

        // Going to use custom service for upload files for now, there is an error in the Mapper conf in FileStorage
        return Ok(await _fileService.UploadFiles(Request.Form.Files.ToArray(), additionalData, ct));
    }

    [HttpPost]
    [Route("delete")]
    public async Task<IActionResult> DeleteMultiplesFiles([FromBody] string[] keys, CancellationToken ct = default)
    {
        if (!keys.Any())
        {
            return BadRequest();
        }

        await _fileService.DeleteMultipleFilesAsync(keys, ct);

        return NoContent();
    }

    [HttpPost]
    public async Task<IActionResult> UploadImages(IFormCollection formData, CancellationToken ct = default)
    {
        var additionalData = formData.Keys.ToDictionary<string, string, string>(key => key, key => formData[key]);
        additionalData.Add("operation_name", UploadingOperationNameImage);

        return Ok(await _fileStorageService.UploadFiles(Request.Form.Files.ToArray(), additionalData, ct));
    }

    [HttpDelete]
    public async Task<IActionResult> DeleteFile(string key, CancellationToken ct = default) =>
        Ok(await _fileStorageService.DeleteFile(key, ct));

    [HttpGet]
    [Route("attachments/{formDataId}")]
    public async Task<FileResult> GetAttachmentsForAFormData(
        [HashedKeyBinder(typeof(FormDataDTO), "Id")]
        int formDataId, CancellationToken ct = default)
    {
        var result = await _fileService.GetAttachmentsZip(formDataId, ct);
        return File(result, "application/zip");
    }
}