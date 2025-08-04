using BBWM.Core;
using BBWM.Core.Data;
using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;
using Microsoft.EntityFrameworkCore;

namespace BBF.Reporting.Dashboard.Api;

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
            new(Routes.Dashboards, AggregatedRoles.Authenticated),
            new(Routes.DashboardCreate, AggregatedRoles.Authenticated),
            new(Routes.DashboardEdit, AggregatedRoles.Authenticated),
            };

        var dashboards = _dbContext.Set<DbModel.Dashboard>()
                //.Include(a => a.ReportRoles).ThenInclude(x => x.Role)
                //.Include(a => a.ReportPermissions).ThenInclude(x => x.Permission)
                //.Where(o => !o.IsDraft)
                .Select(o => new DbModel.Dashboard
                {
                    Id = o.Id,
                    UrlSlug = o.UrlSlug,
                    Name = o.Name,
                    //Access = o.Access,
                    //ReportRoles = o.ReportRoles,
                    //ReportPermissions = o.ReportPermissions
                })
                .ToListAsync().Result;

        foreach (var dashboard in dashboards)
        {
            var pageInfo = new PageInfoDTO($"/app/reporting3/view/{dashboard.UrlSlug}", dashboard.Name);

            pageInfo.ForRole(AggregatedRoles.Authenticated);

            // TODO: we'll need to implement access rules

            //if (report.Access == AggregatedRoles.Authenticated)
            //{
            //    pageInfo.ForRole(AggregatedRoles.Authenticated);
            //}
            //else
            //{
            //    pageInfo
            //       .ForRoles(report.ReportRoles.Select(o => o.Role.Name))
            //       .ForPermissions(report.ReportPermissions.Select(o => o.Permission.Name));
            //}

            pageRoutes.Add(pageInfo);
        }

        return pageRoutes;
    }
}