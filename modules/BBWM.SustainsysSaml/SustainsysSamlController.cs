using BBWM.Core.Membership.Enums;
using BBWM.Core.Membership.Model;

using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

using System.Security.Claims;

namespace BBWM.SustainsysSaml;

[Route("sustainsys-saml")]
public class SustainsysSamlController : Controller
{
    private readonly SignInManager<User> _signInManager;
    private readonly UserManager<User> _userManager;
    private readonly ILogger _logger;

    [TempData]
    public string ErrorMessage { get; set; }

    public SustainsysSamlController(SignInManager<User> signInManager,
        UserManager<User> userManager,
        ILogger<SustainsysSamlController> logger)

    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [AllowAnonymous]
    [HttpGet]
    public IActionResult Index()
    {
        // Request a redirect to the external login provider.
        var redirectUrl = Url.Action("ExternalLoginCallback", "SustainsysSaml", null);
        var properties = _signInManager.ConfigureExternalAuthenticationProperties("Saml2", redirectUrl);
        return Challenge(properties, "Saml2");
    }

    [HttpGet]
    [AllowAnonymous]
    [Route("external-login")]
    public async Task<IActionResult> ExternalLoginCallback(string returnUrl = null, string remoteError = null)
    {
        if (remoteError is not null)
        {
            ErrorMessage = $"Error from external provider: {remoteError}";
            _logger.LogInformation(ErrorMessage);
            return RedirectToAction("login");
        }
        var info = await _signInManager.GetExternalLoginInfoAsync();
        if (info is null)
        {
            _logger.LogInformation("Error loading external login information");
            return RedirectToAction("login");
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
            _logger.LogInformation("User is blocked to sign in using {Name} provider.", info.LoginProvider);
            return RedirectToAction("lockout");
        }
        else
        {
            // If the user does not have an account, then ask the user to create an account.
            ViewData["ReturnUrl"] = returnUrl;
            ViewData["LoginProvider"] = info.LoginProvider;
            var email = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Name);
            _logger.LogInformation("An attempt to create an account with {email} email.", email);

            var user = new User
            {
                UserName = email,
                Email = email,
                FirstName = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.GivenName),
                LastName = info.Principal.FindFirstValue(System.Security.Claims.ClaimTypes.Surname),
                EmailConfirmed = true,
                AccountStatus = AccountStatus.Active
            };
            var createUserResult = await _userManager.CreateAsync(user);
            if (createUserResult.Succeeded)
            {
                await _userManager.AddToRoleAsync(user, Core.Roles.SystemAdminRole);
                var loginResult = await _userManager.AddLoginAsync(user, info);
                if (loginResult.Succeeded)
                {
                    await _signInManager.SignInAsync(user, false);
                    _logger.LogInformation("User created an account using {Name} provider.", info.LoginProvider);
                    return RedirectToLocal(returnUrl);
                }
            }
            AddErrors(createUserResult);
            return View("index");
        }
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
