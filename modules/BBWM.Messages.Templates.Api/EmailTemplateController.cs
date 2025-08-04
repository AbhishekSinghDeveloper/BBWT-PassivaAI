using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Services;
using BBWM.Core.Web;
using BBWM.Core.Web.ModelBinders;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.Messages.Templates.Api;

[Route("api/email-template")]
[Authorize(Roles = Core.Roles.SystemAdminRole)]
public class EmailTemplateController : DataControllerBase<EmailTemplate, EmailTemplateDTO, EmailTemplateDTO>
{
    private readonly IEmailTemplateService _emailTemplatesService;
    private readonly IUserService _userService;

    public EmailTemplateController(
        IDataService dataService,
        IEmailTemplateService emailTemplatesService,
        IUserService userService)
        : base(dataService, emailTemplatesService)
    {
        _emailTemplatesService = emailTemplatesService;
        _userService = userService;
    }

    [HttpGet]
    [Route("check-code")]
    public IActionResult CheckCode(string value, [HashedKeyBinder] int id) =>
        Ok(_emailTemplatesService.CheckEmailTemplateCode(value, id));

    [HttpPost]
    [Route("{id}/test-email")]
    public async Task<IActionResult> SendTestEmail([HashedKeyBinder] int id, string to)
    {
        await _userService.SendTestEmail(to, id, Request.Form.Files);
        return NoContent();
    }
}
