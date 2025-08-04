using BBWM.Core.Services;
using BBWM.Core.Web;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.Messages.Templates.Api;

[Route("api/email-template-parameter")]
[Authorize(Roles = Core.Roles.SystemAdminRole)]
public class EmailTemplateParameterController :
    DataControllerBase<EmailTemplateParameter, EmailTemplateParameterDTO, EmailTemplateParameterDTO>
{
    public EmailTemplateParameterController(IDataService dataService) : base(dataService)
    {
    }
}
