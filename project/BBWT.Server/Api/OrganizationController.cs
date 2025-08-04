using BBWM.Core;
using BBWM.Core.Data;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.ModelHashing;
using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.Filters;
using BBWM.FileStorage;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using System.Security.Claims;

using ClaimTypes = System.Security.Claims.ClaimTypes;
using CoreRoles = BBWM.Core.Roles;

namespace BBWT.Server.Api;

[Route("api/organization")]
[ReadWriteAuthorize(ReadWriteRoles = CoreRoles.SystemAdminRole)]
public class OrganizationController : DataControllerBase<Organization, OrganizationDTO, OrganizationDTO>
{
    private const string UploadLogoImageOperationName = "OrgLogoImageUploading";
    private const string UploadLogoIconOperationName = "OrgLogoIconUploading";

    private readonly IBrandingService _brandingService;
    private readonly IFileStorageService _fileStorageService;
    private readonly IOrganizationService _organizationService;

    public OrganizationController(
        IOrganizationService organizationService,
        IDataService dataService,
        IBrandingService brandingService,
        IFileStorageService fileStorageService,
        IDbContext context) : base(dataService, organizationService)
    {
        _organizationService = organizationService;
        _brandingService = brandingService;
        _fileStorageService = fileStorageService;
    }


    public override async Task<IActionResult> Create(OrganizationDTO dto, [FromServices] IModelHashingService modelHashingService, CancellationToken ct = default)
    {
        var result = await base.Create(dto, modelHashingService, ct);

        // Unbinding files from user and operation name to complete operation
        await CompleteLogoImagesUploading(ct);

        return result;
    }

    public override async Task<IActionResult> Update(OrganizationDTO dto, CancellationToken ct = default)
    {
        // Removing old logo images
        await DeleteOrganizationLogoFiles(dto.Id, dto, ct);

        var result = await base.Update(dto, ct);

        // Unbinding files from user and operation name to complete operation
        await CompleteLogoImagesUploading(ct);

        return result;
    }

    public override async Task<IActionResult> Delete(int id, CancellationToken ct = default)
    {
        // Removing logo image
        await DeleteOrganizationLogoFiles(id, null, ct);

        return await base.Delete(id, ct);
    }

    [HttpPost, Route("exists")]
    public async Task<IActionResult> Exists([FromBody] OrganizationDTO dto, CancellationToken ct)
        => Ok(await _organizationService.Get(dto.Name, dto.Id, ct) is not null);

    [HttpGet, Route("all-plain")]
    // SuperAdmin needs this method from the sys config page (we then need to split UI for system and super admins)
    [Authorize(Roles = Roles.SystemAdminRole + "," + Roles.SuperAdminRole)]
    public async Task<IActionResult> GetAllPlain(CancellationToken ct = default) =>
        Ok(await DataService.GetAll<Organization, OrganizationDTO>(ct));

    [HttpPost, Route("upload-logo-image")]
    [AllowedFormFileFormats(2000000, "image/png", "image/jpeg", "image/gif")]
    public async Task<IActionResult> UploadLogoImage(IFormCollection formData, CancellationToken cancellationToken)
    {
        var files = Request.Form.Files;

        if (files is null) return BadRequest("There is no uploaded file.");
        if (files.Count != 1) return BadRequest("The count of uploaded files should be 1.");

        // Binding files to user and operation name for removing old files
        var additionalData =
            formData.Keys.ToDictionary<string, string, string>(key => key, key => formData[key]);
        additionalData.Add("max_size", "10000");
        additionalData.Add("thumbnail_size", "400");
        additionalData.Add("user_id", User.FindFirstValue(ClaimTypes.NameIdentifier));
        additionalData.Add("operation_name", UploadLogoImageOperationName);

        return Ok((await _fileStorageService.UploadFiles(files.ToArray(), additionalData, cancellationToken)).SuccessfullyUploadedFiles[0]);
    }

    [HttpPost, Route("upload-logo-icon")]
    [AllowedFormFileFormats(500000, "image/x-icon")]
    public async Task<IActionResult> UploadLogoIcon(IFormCollection formData, CancellationToken cancellationToken)
    {
        var files = Request.Form.Files;

        if (files is null) return BadRequest("There is no uploaded file.");
        if (files.Count != 1) return BadRequest("The count of uploaded files should be 1.");

        // Binding files to user and operation name for removing old files
        var additionalData =
            formData.Keys.ToDictionary<string, string, string>(key => key, key => formData[key]);
        additionalData.Add("max_size", "50");
        additionalData.Add("thumbnail_size", "50");
        additionalData.Add("user_id", User.FindFirstValue(ClaimTypes.NameIdentifier));
        additionalData.Add("operation_name", UploadLogoIconOperationName);

        return Ok((await _fileStorageService.UploadFiles(files.ToArray(), additionalData, cancellationToken)).SuccessfullyUploadedFiles[0]);
    }

    private async Task DeleteOrganizationLogoFiles(int id, OrganizationDTO dto = null, CancellationToken cancellationToken = default)
    {
        var organization = await DataService.Get<Organization, OrganizationDTO>(id, query => query.Include(o => o.Branding), cancellationToken);

        if (organization?.Branding?.LogoIconId is not null && organization.Branding.LogoIconId != dto?.Branding?.LogoIconId)
            await _brandingService.DeleteLogoIcon(organization.Branding.Id, cancellationToken);

        if (organization?.Branding?.LogoImageId is not null && organization.Branding.LogoImageId != dto?.Branding?.LogoImageId)
            await _brandingService.DeleteLogoImage(organization.Branding.Id, cancellationToken);
    }

    private async Task CompleteLogoImagesUploading(CancellationToken cancellationToken = default)
    {
        await _fileStorageService.CompleteUsersFilesUploadingOperation(
            User.FindFirstValue(ClaimTypes.NameIdentifier), UploadLogoImageOperationName, cancellationToken);
        await _fileStorageService.CompleteUsersFilesUploadingOperation(
            User.FindFirstValue(ClaimTypes.NameIdentifier), UploadLogoIconOperationName, cancellationToken);
    }
}
