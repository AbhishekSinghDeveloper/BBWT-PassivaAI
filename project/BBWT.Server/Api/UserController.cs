using BBWM.Core.Exceptions;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Extensions;
using BBWM.Core.Membership.Filters;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.Filters;
using BBWM.FileStorage;
using BBWM.FormIO.DTO;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Globalization;
using System.Security.Claims;
using ClaimTypes = System.Security.Claims.ClaimTypes;
using CoreRoles = BBWM.Core.Roles;

namespace BBWT.Server.Api;

[Produces("application/json")]
[Route("api/user")]
[ReadWriteAuthorize(ReadWriteRoles = CoreRoles.SystemAdminRole)]
public class UserController : DataControllerBase<User, UserDTO, UserDTO, string>
{
    private const string UploadAvatarImageOperationName = "UserAvatarImageUploading";
    private readonly IUserDataService _userDataService;
    private readonly IUserService _userService;
    private readonly UserManager<User> _userManager;
    private readonly IFileStorageService _fileStorageService;

    public UserController(
        IDataService dataService,
        IUserDataService userDataService,
        IUserService userService,
        UserManager<User> userManager,
        IFileStorageService fileStorageService) : base(dataService, userDataService)
    {
        _userService = userService;
        _userDataService = userDataService;
        _fileStorageService = fileStorageService;
        _userManager = userManager;
    }

    [ResponseCache(NoStore = true)]
    public override Task<IActionResult> Get(string id, CancellationToken ct = default)
        => base.Get(id, ct);

    public override async Task<IActionResult> Update(UserDTO dto, CancellationToken ct = default)
    {
        var existingUser = await _userManager.FindByIdAsync(dto.Id);

        if (existingUser is null)
            throw new EntityNotFoundException("User not found.");

        var oldEmail = existingUser.Email;

        var userByEmail = await _userManager.FindByEmailAsync(dto.Id);
        if (userByEmail is not null && userByEmail.Id != dto.Id)
            throw new InvalidModelException("User with specified email already exists.");

        var result = await _userDataService.Update(dto, ct);

        if (oldEmail != dto.Email)
            await _userService.NotifyUserOnEmailChanged(existingUser.Id, oldEmail, ct);

        return Ok(result);
    }

    [HttpPost]
    [Route("invite")]
    public async Task<IActionResult> Invite([FromBody] UserDTO user, CancellationToken cancellationToken = default) =>
        Ok(await _userService.Invite(user, cancellationToken));

    [HttpPost]
    [Route("{userId}/resend-invitation")]
    public async Task<IActionResult> ResendInvitation(string userId, CancellationToken cancellationToken = default)
    {
        if (!await _userDataService.Exists(userId, cancellationToken))
            throw new EntityNotFoundException("User not found.");

        await _userService.ResendInvitation(userId, cancellationToken);
        return NoContent();
    }

    [HttpPost]
    [Route("{userId}/resend-email-confirmation")]
    public async Task<IActionResult> ResendEmailConfirmation(string userId, CancellationToken cancellationToken)
    {
        if (!await _userDataService.Exists(userId, cancellationToken))
            throw new EntityNotFoundException("User not found.");

        await _userService.ResendEmailConfirmation(userId, cancellationToken);
        return NoContent();
    }

    [Route("me")]
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetLoggedUser()
    {
        if (!User.Identity.IsAuthenticated) return Ok(null);

        await _userService.RefreshTwoFactorSetupClaims();
        var userDto = await _userDataService.Get(User.FindFirstValue(ClaimTypes.NameIdentifier));
        if (userDto is not null)
        {
            userDto.IsUserRequiredSetupTwoFactor = HttpContext.User.IsUserRequiredSetupTwoFactor();
        }
        return Ok(userDto);
    }

    [HttpPost, Route("me")]
    [Authorize]
    public async Task<IActionResult> UpdateLoggedUser(
        [FromBody] UserDTO dto,
        [FromServices] SignInManager<User> signInManager,
        CancellationToken cancellationToken)
    {
        // To ensure we update the logged user, for security purpose
        dto.Id = User.FindFirstValue(ClaimTypes.NameIdentifier);

        var oldEmail = (await _userManager.FindByIdAsync(dto.Id)).Email;
        var isNewEmail = string.Compare(oldEmail, dto.Email, true, CultureInfo.InvariantCulture) != 0;

        if (isNewEmail && await _userManager.FindByEmailAsync(dto.Email) is not null)
            throw new BusinessException("The email is already in use.");

        // Removing old avatar image
        await DeleteUserAvatarFile(dto.Id, dto, cancellationToken);

        var result = await _userDataService.Update(dto, cancellationToken);

        if (isNewEmail)
            await _userService.NotifyUserOnEmailChanged(dto.Id, oldEmail, cancellationToken);

        // Unbinding file from user and operation name to complete operation
        await CompleteAvatarImageUploading(cancellationToken);

        if (isNewEmail)
        {
            await signInManager.SignOutAsync();
            return Unauthorized(new { NewEmail = true });
        }

        return Ok(result);
    }

    [HttpPost]
    [Route("me/{impersonatedUserId}/impersonate")]
    public async Task<IActionResult> ImpersonateCurrentUserAsUser(string impersonatedUserId, CancellationToken cancellationToken = default)
    {
        if (!await _userDataService.Exists(impersonatedUserId, cancellationToken))
            throw new EntityNotFoundException("Impersonated user not found.");

        return Ok(await _userService.Impersonate(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            impersonatedUserId,
            cancellationToken
        ));
    }

    [HttpPost]
    [Route("me/stop-impersonation")]
    [Authorize]
    public async Task<IActionResult> StopCurrentUserImpersonation() =>
        Ok(await _userService.StopImpersonation(User));

    [HttpGet]
    [Route("me/is-impersonating")]
    [IgnoreSetup2FaCheck]
    [Authorize]
    public async Task<IActionResult> IsCurrentUserImpersonating() =>
        Ok(await _userService.IsUserImpersonating(User));

    [Route("me/{impersonatedUserId}/can-impersonate")]
    [HttpGet]
    public async Task<IActionResult> CanCurrentUserImpersonateUser(string impersonatedUserId) =>
        Ok(await _userService.CanImpersonate(
            User.FindFirstValue(ClaimTypes.NameIdentifier),
            impersonatedUserId));

    [HttpGet]
    [IgnoreSetup2FaCheck]
    [Authorize]
    public async Task<IActionResult> GetAll()
    {
        var results = await this._userDataService.GetPage(new BBWM.Core.Filters.QueryCommand());
        return Ok(results.Items);
    }

    [HttpPost, Route("{userId}/approve")]
    public async Task<IActionResult> Approve(string userId, CancellationToken cancellationToken)
    {
        if (!await _userDataService.Exists(userId, cancellationToken))
            throw new EntityNotFoundException("User not found.");

        await _userService.Approve(userId, cancellationToken);
        return NoContent();
    }

    [HttpPost, Route("{userId}/toggle-locking")]
    public async Task<IActionResult> ToggleLocking(string userId, CancellationToken cancellationToken)
    {
        if (!await _userDataService.Exists(userId, cancellationToken))
            throw new EntityNotFoundException("User not found.");

        await _userService.ToggleLocking(userId, cancellationToken);
        return NoContent();
    }

    [HttpPost, Route("{userId}/toggle-deleting")]
    public async Task<IActionResult> ToggleDeleting(string userId, CancellationToken cancellationToken)
    {
        if (!await _userDataService.Exists(userId, cancellationToken))
            throw new EntityNotFoundException("User not found.");

        await _userService.ToggleDeleting(userId, cancellationToken);
        return NoContent();
    }

    [HttpGet, Route("{userId}/roles"), ResponseCache(NoStore = true)]
    public async Task<IActionResult> GetUserRoles(string userId, CancellationToken cancellationToken)
    {
        var user = await _userDataService.Get(userId, cancellationToken);

        if (user is null)
            throw new EntityNotFoundException("User not found.");

        return Ok(user.Roles);
    }

    [HttpGet]
    [Route("all-roles")]
    public async Task<IActionResult> GetAllRoles() => Ok(await _userDataService.GetAllRoles());

    [HttpGet]
    [Route("all-groups")]
    public async Task<IActionResult> GetAllGroups() => Ok(await _userDataService.GetAllGroups());

    [HttpGet, Route("{id}/email")]
    [AllowAnonymous]
    public async Task<IActionResult> GetEmail(string id, CancellationToken cancellationToken)
    {
        var user = await DataService.Get<User, UserDTO, string>(id, cancellationToken);

        if (user is null)
            throw new EntityNotFoundException("User not found.");

        return Ok(user.Email);
    }

    [HttpPost]
    [Route("replace-users-roles")]
    public async Task<IActionResult> ReplaceUsersRoles([FromBody] UsersRolesReplacementDTO dto, CancellationToken cancellationToken = default)
        => Ok(await _userDataService.ReplaceUsersRoles(dto, cancellationToken));

    [HttpPost]
    [Route("replace-users-groups")]
    public async Task<IActionResult> ReplaceUsersGroups([FromBody] UsersGroupsReplacementDTO dto, CancellationToken cancellationToken = default)
        => Ok(await _userDataService.ReplaceUsersGroups(dto, cancellationToken));

    [HttpPost, Route("upload-avatar-image")]
    [Authorize]
    [AllowedFormFileFormats(300000, "image/png", "image/jpeg", "image/gif")]
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
        additionalData.Add("operation_name", UploadAvatarImageOperationName);

        return Ok((await _fileStorageService.UploadFiles(files.ToArray(), additionalData, cancellationToken)).SuccessfullyUploadedFiles[0]);
    }

    [HttpGet, Route("get-avatar-image")]
    [Authorize]
    public async Task<IActionResult> GetLogoImage(CancellationToken cancellationToken)
    {
        var user = await _userDataService.Get(User.FindFirstValue(ClaimTypes.NameIdentifier), cancellationToken);
        if (!user.AvatarImageId.HasValue)
        {
            return NoContent();
        }
        return Ok(await _fileStorageService.Get(user.AvatarImageId.Value));
    }

    [HttpGet, Route("usersignature/{userId}")]
    [Authorize]
    public async Task<IActionResult> GetUserSignature(string userId, CancellationToken cancellationToken = default)
    {
        var userSignature = await _userDataService.GetUserSignature(userId, cancellationToken);

        return Ok(userSignature);
    }

    [HttpPost, Route("usersignature/{userId}")]
    [Authorize]
    public async Task<IActionResult> SetUserSignature(string userId, [FromBody] FormDefinitionParameters signature, CancellationToken cancellationToken = default)
    {
        var signatureUpdated = await _userDataService.SetUserSignature(userId, signature.ParameterString.FirstOrDefault() ?? string.Empty, cancellationToken);

        return Ok(signatureUpdated);
    }


    private async Task DeleteUserAvatarFile(string id, UserDTO dto, CancellationToken cancellationToken = default)
    {
        var user = await _userDataService.Get(id, cancellationToken);

        if (user.AvatarImageId is not null && user.AvatarImageId != dto.AvatarImageId)
            await _fileStorageService.DeleteFile((int)user.AvatarImageId, cancellationToken);
    }

    private async Task CompleteAvatarImageUploading(CancellationToken cancellationToken = default) =>
        await _fileStorageService.CompleteUsersFilesUploadingOperation(
            User.FindFirstValue(ClaimTypes.NameIdentifier), UploadAvatarImageOperationName, cancellationToken);
}
