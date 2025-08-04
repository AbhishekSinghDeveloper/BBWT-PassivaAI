using BBWM.Core.Membership.Enums;
using BBWM.Core.Membership.Model;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Security.Claims;

namespace BBWM.SsoProviders;

[Route("SsoProvider")]
public class SsoProviderController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly ILogger _logger;

    public SsoProviderController(SignInManager<User> signInManager,
        UserManager<User> userManager,
        ILogger<SsoProviderController> logger)

    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    [Route("{id}")]
    public IActionResult Index(string id)
    {
        // Request a redirect to the external login provider.
        var redirectUrl = Url.Action("external-login");
        var properties = _signInManager.ConfigureExternalAuthenticationProperties(id, redirectUrl);
        return Challenge(properties, id);
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("external-login")]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
    {
        if (remoteError is not null)
        {
            var error = $"Error from external provider: {remoteError}";
            _logger.LogInformation(error);
            return RedirectToLoginWithError(error);
        }
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            var error = "Error loading external login information";
            _logger.LogInformation(error);
            return RedirectToLoginWithError(error);
        }

        // Check if user with such email exists and his status
        var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Email);
        var existingUser = await _userManager.FindByEmailAsync(email);
        if (existingUser is not null)
        {
            switch (existingUser.AccountStatus)
            {
                case AccountStatus.Invited:
                    return RedirectToLoginWithError("An invitation has not been accepted yet.");
                case AccountStatus.Unapproved:
                    return RedirectToLoginWithError("Account is not approved yet.");
                case AccountStatus.Unverified:
                    return RedirectToLoginWithError("Email address is not verified yet.");
                case AccountStatus.Suspended:
                    return RedirectToLoginWithError("Account is suspended.");
                case AccountStatus.Deleted:
                    return RedirectToLoginWithError("Account is deleted.");
            }
        }

        // Sign in the user with this external login provider if the user already has a login.
        var result = await _signInManager.ExternalLoginSignInAsync(info.LoginProvider, info.ProviderKey, isPersistent: false, bypassTwoFactor: true);
        if (result.Succeeded)
        {
            _logger.LogInformation("User logged in with {Name} provider.", info.LoginProvider);
            return Redirect("~/");
        }
        if (result.IsLockedOut)
        {
            var error = "User is blocked to sign in using {Name} provider.";
            _logger.LogInformation(error, info.LoginProvider);
            return RedirectToLoginWithError(error);
        }
        _logger.LogInformation("An attempt to create an account with {email} email.", email);

        var user = new User
        {
            UserName = email,
            Email = email,
            FirstName = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.GivenName),
            LastName = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Surname),
            EmailConfirmed = true,
            AccountStatus = AccountStatus.Active,
            SsoProvider =
                info.LoginProvider == "Google" ? (int)SsoProvider.Google :
                info.LoginProvider == "Facebook" ? (int)SsoProvider.Facebook :
                info.LoginProvider == "LinkedIn" ? (int)SsoProvider.LinkedIn : null

        };
        var createUserResult = await _userManager.CreateAsync(user);
        if (createUserResult.Succeeded)
        {
            await _userManager.AddToRoleAsync(user, Core.Roles.SystemTester);
            var loginResult = await _userManager.AddLoginAsync(user, info);
            if (loginResult.Succeeded)
            {
                await _signInManager.SignInAsync(user, false);
                _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                return RedirectToLocal(returnUrl);
            }
        }
        AddErrors(createUserResult);

        return RedirectToLoginWithError(createUserResult.Errors.Any() ? createUserResult.Errors.ToList()[0].Description : "");
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("login")]
    public async Task<IActionResult> Login(string returnUrl = null)
    {
        // Clear the existing external cookie to ensure a clean login process
        await HttpContext.SignOutAsync(IdentityConstants.ExternalScheme);

        ViewData["ReturnUrl"] = returnUrl;
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("lockout")]
    public IActionResult Lockout()
    {
        return View();
    }

    private IActionResult RedirectToLoginWithError(string error)
    {
        var option = new CookieOptions();
        option.Expires = System.DateTime.Now.AddMinutes(10);

        Response.Cookies.Append("sso-provider-login-error", "SSO Login Error. " + error, option);

        return Redirect("~/");
    }

    private IActionResult RedirectToLocal(string returnUrl)
    {
        if (Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }
        else
        {
            return Redirect("~/");
        }
    }

    private void AddErrors(IdentityResult result)
    {
        foreach (var error in result.Errors)
        {
            ModelState.AddModelError(string.Empty, error.Description);
        }
    }

}
