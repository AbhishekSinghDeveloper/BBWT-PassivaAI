using BBWM.Core;
using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;

using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Demo.ModuleLinkage;

public class RouteRolesModuleLinkage : IRouteRolesModuleLinkage
{
    public List<PageInfoDTO> GetRouteRoles(IServiceScope serviceScope) =>
        new List<PageInfoDTO>
        {
                new PageInfoDTO(Routes.GridLocal, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.GridFilter, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.GridMasterDetail.Inline, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.GridMasterDetail.Page, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.GridMasterDetail.Popup, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.GridMasterDetail.Create, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.GridMasterDetail.Edit, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.GridMasterDetail.Details, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.IdHashing, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.IdHashingDetails, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Impersonation, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.ImageUploader, AggregatedRoles.Authenticated),

                new PageInfoDTO(Routes.Guidelines.General, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Guidelines.Basic, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Guidelines.Headings, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Guidelines.Lists, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Guidelines.Inputs, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Guidelines.Buttons, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Guidelines.Calendar, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Guidelines.Disabled, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Guidelines.Links, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Guidelines.Search, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Guidelines.Tabs, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Guidelines.Panels, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Guidelines.Dialogs, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Guidelines.Grids, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Guidelines.Tree, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Guidelines.Pdf, AggregatedRoles.Authenticated),

                new PageInfoDTO(Routes.Raygun, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.Culture, AggregatedRoles.Authenticated),
                new PageInfoDTO(Routes.DisabledControls, AggregatedRoles.Authenticated)
        };
}
