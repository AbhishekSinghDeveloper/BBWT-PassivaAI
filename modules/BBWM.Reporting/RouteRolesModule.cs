using BBWM.Core;
using BBWM.Core.Data;
using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;
using BBWM.Reporting.Model;
using Microsoft.EntityFrameworkCore;

namespace BBWM.Reporting;

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
                new PageInfoDTO(Routes.Reports, new[] { Core.Roles.SuperAdminRole, Roles.ReportEditorRole }),
                new PageInfoDTO(Routes.ReportCreation, new[] { Core.Roles.SuperAdminRole, Roles.ReportEditorRole }),
                new PageInfoDTO(Routes.ReportEditing, new[] { Core.Roles.SuperAdminRole, Roles.ReportEditorRole }),
                new PageInfoDTO(Routes.NamedQueries, new[] { Core.Roles.SuperAdminRole, Roles.ReportEditorRole }),
                new PageInfoDTO(Routes.NamedQueryCreation, new[] { Core.Roles.SuperAdminRole, Roles.ReportEditorRole }),
                new PageInfoDTO(Routes.NamedQueryEditing, new[] { Core.Roles.SuperAdminRole, Roles.ReportEditorRole })
            };

        var reports = _dbContext.Set<Report>()
                .Include(a => a.ReportRoles).ThenInclude(x => x.Role)
                .Include(a => a.ReportPermissions).ThenInclude(x => x.Permission)
                .Where(o => !o.IsDraft)
                .Select(o => new Report
                {
                    Id = o.Id,
                    UrlSlug = o.UrlSlug,
                    Name = o.Name,
                    Access = o.Access,
                    ReportRoles = o.ReportRoles,
                    ReportPermissions = o.ReportPermissions
                })
                .ToListAsync().Result;

        foreach (var report in reports)
        {
            var pageInfo = new PageInfoDTO($"/app/reporting/view/{report.UrlSlug}", report.Name);

            if (report.Access == AggregatedRoles.Authenticated)
            {
                pageInfo.ForRole(AggregatedRoles.Authenticated);
            }
            else
            {
                pageInfo
                   .ForRoles(report.ReportRoles.Select(o => o.Role.Name))
                   .ForPermissions(report.ReportPermissions.Select(o => o.Permission.Name));
            }

            pageRoutes.Add(pageInfo);
        }

        return pageRoutes;
    }
}