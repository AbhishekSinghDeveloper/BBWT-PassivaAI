using BBWM.Core;
using BBWM.Core.Data;
using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;
using Microsoft.EntityFrameworkCore;

namespace BBWM.FormIO;

public class RouteRolesModule : IRouteRolesModule
{
    private readonly IDbContext _dbContext;

    public RouteRolesModule(IDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public List<PageInfoDTO> GetRouteRoles()
    {
        var pageRoutes = new List<PageInfoDTO>() {
                new PageInfoDTO(Routes.FormIOBuilder, new[] { AggregatedRoles.Authenticated }),
                new PageInfoDTO(Routes.FormIOList, new[] { AggregatedRoles.Authenticated}),
                new PageInfoDTO(Routes.FormIODisplay, new[] { AggregatedRoles.Authenticated}),
                new PageInfoDTO(Routes.FormioPDFGenerator, new[] { AggregatedRoles.Authenticated}),
                new PageInfoDTO(Routes.FormioInstances, new[] { AggregatedRoles.Authenticated}),
                new PageInfoDTO(Routes.UserSignature, new[] { AggregatedRoles.Authenticated}),
                new PageInfoDTO(Routes.FormIODataExplorer, new[] { AggregatedRoles.Authenticated}),
                new PageInfoDTO(Routes.FormioDisabled, new[] { AggregatedRoles.Authenticated}),
                new PageInfoDTO(Routes.FormioDetails, new[] { AggregatedRoles.Authenticated}),
                new PageInfoDTO(Routes.FormioRequests, new[] { AggregatedRoles.Authenticated}),

                new PageInfoDTO(Routes.FormioMultiUserList, new[] { AggregatedRoles.Authenticated}),
                new PageInfoDTO(Routes.FormioMultiUserStages, new[] { AggregatedRoles.Authenticated}),
                new PageInfoDTO(Routes.FormioMultiUserDisplay, new[] { AggregatedRoles.Authenticated}),
                new PageInfoDTO(Routes.FormioMultiUserDisplayExternal, new[] { AggregatedRoles.Anyone}),
                new PageInfoDTO(Routes.FormIOSurveyList, new [] {AggregatedRoles.Authenticated}),
                new PageInfoDTO(Routes.FormIOSurveyPending, new [] { AggregatedRoles.Authenticated}),
                new PageInfoDTO(Routes.FormIOInstancesExplorer, new [] { AggregatedRoles.Authenticated}),

                new PageInfoDTO(Routes.FormIOCategory, new [] { AggregatedRoles.Authenticated}),
        };
        return pageRoutes;
    }
}