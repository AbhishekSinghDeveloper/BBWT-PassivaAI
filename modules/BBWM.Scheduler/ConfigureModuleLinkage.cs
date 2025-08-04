using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership;
using BBWM.Menu.DTO;
using BBWM.Menu;
using Microsoft.Extensions.DependencyInjection;
using BBWM.Core.ModuleLinker;
using Microsoft.EntityFrameworkCore;
using BBWM.Scheduler.ModelConfiguration;
using System.Reflection.Emit;

namespace BBWM.Scheduler;

public class ConfigureModuleLinkage
    : IMenuModuleLinkage,
      IRouteRolesModuleLinkage,
        IDbModelCreateModuleLinkage
{
    public void CreateInitialMenuItems(List<MenuDTO> menu, MenuLinkageRootMenus rootMenus)
    {
        rootMenus.OperationalAdmin.Children.Add(new MenuDTO
        {
            Label = "Quartz Scheduler",
            Icon = "schedule",
            Children = new List<MenuDTO> {
                new MenuDTO(Routes.SchedulerDashboard, "Dashboard"),
                new MenuDTO(Routes.SchedulerJobs, "Jobs"),
                new MenuDTO(Routes.SchedulerRecurringJobs, "Recurring Jobs"),
                new MenuDTO(Routes.SchedulerRetries, "Retries"),
                new MenuDTO(Routes.SchedulerServers, "Servers")
            }
        });
    }

    public List<PageInfoDTO> GetRouteRoles(IServiceScope serviceScope)
        => new List<PageInfoDTO>
        {
            new PageInfoDTO(Routes.SchedulerDashboard, new List<string>{ Core.Roles.SuperAdminRole, Core.Roles.SystemAdminRole }),
            new PageInfoDTO(Routes.SchedulerJobs, new List<string>{ Core.Roles.SuperAdminRole, Core.Roles.SystemAdminRole }),
            new PageInfoDTO(Routes.SchedulerRetries, new List<string>{ Core.Roles.SuperAdminRole, Core.Roles.SystemAdminRole }),
            new PageInfoDTO(Routes.SchedulerRecurringJobs, new List<string>{ Core.Roles.SuperAdminRole, Core.Roles.SystemAdminRole }),
            new PageInfoDTO(Routes.SchedulerServers, new List<string>{ Core.Roles.SuperAdminRole, Core.Roles.SystemAdminRole }),
        };

    public void OnModelCreating(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new JobExecutionDetailsConfiguration());
    }
}
