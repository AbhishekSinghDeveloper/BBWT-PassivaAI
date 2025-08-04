using BBWM.Core.Membership.Model;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace BBWM.SAML
{
    [Route("saml")]
    public class SamlController : Controller
    {
        private readonly SignInManager<User> _signInManager;
        private readonly UserManager<User> _userManager;
        private readonly ISamlService _samlService;

        public SamlController(ISamlService samlService, SignInManager<User> signInManager, UserManager<User> userManager)
        {
            _samlService = samlService;
            _signInManager = signInManager;
            _userManager = userManager;
        }

        [AllowAnonymous]
        [HttpGet]
        public IActionResult Index()
        {
            var redirectUrl = _samlService.RedirectToProvider();
            if (!string.IsNullOrEmpty(redirectUrl))
            {
                return Redirect(redirectUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("login")]
        [IgnoreAntiforgeryToken]
        public async Task<IActionResult> Login()
        {
            var form = Request.Form;
            var samlUser = _samlService.ParseResponse(form["SAMLResponse"]);
            if (samlUser == null || string.IsNullOrEmpty(samlUser.Username)) return BadRequest("Can't parse response");

            var user = await _userManager.FindByNameAsync(samlUser.Username);
            if (user == null)
            {
                user = new User
                {
                    Email = samlUser.Email,
                    UserName = samlUser.Username,
                    FirstName = samlUser.FirstName,
                    LastName = samlUser.LastName,
                    PhoneNumber = samlUser.Phone
                };
                await _userManager.CreateAsync(user);
                await _userManager.AddLoginAsync(user, new UserLoginInfo("SAML", samlUser.Email, samlUser.Username));
            }

            await _signInManager.SignInAsync(user, false);

            return RedirectToAction("Index", "Home");
        }
    }
}