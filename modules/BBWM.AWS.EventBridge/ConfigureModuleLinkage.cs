using BBWM.AWS.EventBridge.Interfaces;
using BBWM.AWS.EventBridge.Model;
using BBWM.Core.Membership;
using BBWM.Core.Membership.DTO;
using BBWM.Core.ModuleLinker;
using BBWM.Menu;
using BBWM.Menu.DTO;

using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace BBWM.AWS.EventBridge;

public class ConfigureModuleLinkage
    : IConfigureModuleLinkage,
      IMenuModuleLinkage,
      IRouteRolesModuleLinkage,
      IDbModelCreateModuleLinkage
{
    public void ConfigureModule(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope();
        var jobService = scope.ServiceProvider.GetService<IAwsEventBridgeJobService>();

        if (jobService is not null)
        {
            var jobs = EventBridgeModuleLinker.GetEventBridgeJobImplementors();
            jobs.ForEach(t =>
            {
                var register = typeof(IAwsEventBridgeJobService)
                    .GetMethod(nameof(IAwsEventBridgeJobService.RegisterJob))
                    .MakeGenericMethod(t.JobType);
                register.Invoke(jobService, new object[0]);
            });
        }

        var hostAppLifeTime = scope.ServiceProvider.GetService<IHostApplicationLifetime>();
        hostAppLifeTime.ApplicationStopped.Register(JobExecutionWrapper.CancelByShutdown);
    }

    public void CreateInitialMenuItems(List<MenuDTO> menu, MenuLinkageRootMenus rootMenus)
    {
        rootMenus.OperationalAdmin.Children.Add(new MenuDTO
        {
            Label = "AWS Scheduler",
            Icon = "schedule",
            Children = new List<MenuDTO> {
                new MenuDTO(Routes.Jobs, "build"),
                new MenuDTO(Routes.History, "history"),
                new MenuDTO(Routes.Tech, "system_update")
            }
        });
    }

    public List<PageInfoDTO> GetRouteRoles(IServiceScope serviceScope)
        => new List<PageInfoDTO>
        {
            new PageInfoDTO(Routes.Jobs, new List<string>{ Core.Roles.SuperAdminRole, Core.Roles.SystemAdminRole }),
            new PageInfoDTO(Routes.History, new List<string>{ Core.Roles.SuperAdminRole, Core.Roles.SystemAdminRole }),
            new PageInfoDTO(Routes.Tech, Core.Roles.SystemAdminRole)
        };

    public void OnModelCreating(ModelBuilder builder)
    {
        // Registering models of the module for the main project's DB context
        builder.Entity<EventBridgeJob>();
        builder.Entity<EventBridgeJobHistory>();
        builder.Entity<EventBridgeRunningJob>();

        // Registering models configurations of the module for the main project's DB context
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
