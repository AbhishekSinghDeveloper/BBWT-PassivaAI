using BBWM.Core.Web.Filters;
using Microsoft.AspNetCore.Mvc;

namespace BBWM.Reporting.Controllers
{
    [Route("api/reporting/named-query")]
    [ReadWriteAuthorize(ReadWriteRoles = Core.Roles.SuperAdminRole + "," + Roles.ReportEditorRole)]
    public class NamedQueryController
    {
    }
}
