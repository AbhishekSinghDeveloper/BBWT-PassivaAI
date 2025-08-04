using BBWM.Core.Membership;
using BBWM.Core.Membership.Enums;
using BBWM.Core.Membership.Utils;

namespace BBWT.Services;

public class ApiAccessModelGetter : IApiAccessModelGetter
{
    public ApiAccessModel GetApiAccessModel() =>
        PermissionsExtractor.GetPermissionNamesOfClass(typeof(Permissions)).Any() ?
        ApiAccessModel.PermissionBased :
        ApiAccessModel.RoleBased;
}
