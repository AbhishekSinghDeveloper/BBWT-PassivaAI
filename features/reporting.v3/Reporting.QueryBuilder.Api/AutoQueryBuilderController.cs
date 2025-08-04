using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BBF.Reporting.QueryBuilder.Api;

[Route("api/reporting3/query/auto")]
[Authorize]
public class AutoQueryBuilderController : BBWM.Core.Web.ControllerBase
{
}