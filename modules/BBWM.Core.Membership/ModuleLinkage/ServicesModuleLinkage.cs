using Autofac;

using BBWM.Core.Autofac;
using BBWM.Core.Exceptions;
using BBWM.Core.Membership.Authorization;
using BBWM.Core.Membership.DTO;
using BBWM.Core.Membership.Interfaces;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Services;
using BBWM.Core.Membership.SystemSettings;
using BBWM.Core.ModelHashing;
using BBWM.Core.ModuleLinker;
using BBWM.Metadata;
using BBWM.SystemSettings;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BBWM.Core.Membership.ModuleLinkage;

public class ServiceModuleLinkage :
    IServicesModuleLinkage,
    IDependenciesModuleLinkage,
    IConfigureModuleLinkage
{
    private readonly string membershipSection = "MembershipSettings";
    private readonly string userLoginSection = "UserLoginSettings";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IUserClaimsPrincipalFactory<User>, BBWT3UserClaimsPrincipalFactory>();

        // Config file secions
        var section = configuration.GetSection(membershipSection);
        if (section.Get<MembershipSettings>() is null)
            throw new EmptyConfigurationSectionException(membershipSection);
        if (string.IsNullOrWhiteSpace(section.Get<MembershipSettings>().RolesFilePath))
            throw new EmptyConfigurationSectionException($"{membershipSection}.{nameof(MembershipSettings.RolesFilePath)}");
        services.Configure<MembershipSettings>(section);

        section = configuration.GetSection(userLoginSection);
        services.Configure<UserLoginSettings>(section);

        services.Configure<DataProtectionTokenProviderOptions>(options => { });
    }

    public void RegisterDependencies(ContainerBuilder builder)
    {
        builder.RegisterService<IPwnedPasswordProvider, PwnedPasswordProvider>();
        builder.RegisterService<IMetadataService, MetadataService<Model.Metadata, User>>();

        // Module linkage
        builder.RegisterService<IRouteRolesModule, RouteRolesModule>();
    }

    public void ConfigureModule(IApplicationBuilder app)
    {
        app.RegisterSection<UserPasswordSettings>("UserPasswordSettings")
           .RegisterSection<FailedAttemptsPasswordSettings>("FailedAttemptsPassword")
           .RegisterSection<UserSessionSettings>("UserSessionSettings")
           .RegisterSection<RegistrationSettings>("RegistrationSettings")
           .RegisterSection<TwoFactorSettings>("TwoFactorSettings");

        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var modelHashingService = serviceScope.ServiceProvider.GetService<IModelHashingService>();
        // Models hashing settings
        modelHashingService.IgnoreModelHashing<LoginAuditDTO>();
        modelHashingService.IgnorePropertiesHashing<LoginAuditDTO>(a => a.Id);
        modelHashingService.ManualPropertyHashing<OrganizationDTO, Organization>(e => e.BrandingId);
        modelHashingService.ManualPropertyHashing<OrganizationDTO, Organization>(c => c.Id);
        modelHashingService.ManualPropertyHashing<BrandingDTO, Organization>(b => b.Id);
    }
}
