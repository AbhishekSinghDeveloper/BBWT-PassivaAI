using Autofac;
using BBWM.Core.ModuleLinker;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Quartz;
using BBWM.Scheduler.Jobs;
using Quartz.Spi;
using BBWM.Scheduler.Model;

namespace BBWM.Scheduler
{
    public class ModuleLinkage : IServicesModuleLinkage, IDependenciesModuleLinkage, IDbModelCreateModuleLinkage
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddSignalR();
            services.ConfigureQuartzScheduler(configuration);
        }

        public void RegisterDependencies(ContainerBuilder builder)
        {
            builder.RegisterType<TestReportingJob3>().AsSelf().InstancePerDependency();
            builder.RegisterType<TestReportingJob>().AsSelf().InstancePerDependency();

            builder.Register(c =>
            {
                var schedulerFactory = c.Resolve<ISchedulerFactory>();
                var scheduler = schedulerFactory.GetScheduler().Result;

                scheduler.JobFactory = new AutofacJobFactory(c.Resolve<ILifetimeScope>());
                return scheduler;
            }).As<IScheduler>().SingleInstance();
        }

        public class AutofacJobFactory : IJobFactory
        {
            private readonly ILifetimeScope _lifetimeScope;

            public AutofacJobFactory(ILifetimeScope lifetimeScope)
            {
                _lifetimeScope = lifetimeScope;
            }

            public IJob NewJob(TriggerFiredBundle bundle, IScheduler scheduler)
            {
                return (IJob)_lifetimeScope.Resolve(bundle.JobDetail.JobType);
            }

            public void ReturnJob(IJob job)
            {
                // Optionally, handle job disposal if necessary
            }
        }

        public void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<JobRunDetails>();
        }
    }
}
