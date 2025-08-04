using Autofac;
using BBWM.Core.Autofac;
using BBWM.Core.Membership;
using BBWM.Core.ModuleLinker;
using BBWM.FormIO.Connectors.ReportingV2;
using BBWM.FormIO.Interfaces;
using BBWM.FormIO.Models;
using BBWM.FormIO.Services;
using BBWM.SystemSettings;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using BBWM.Reporting;
using BBWM.Core.Data;
using BBWM.Core.Membership.Model;
using BBWM.FormIO.Interfaces.FormVersioningInterfaces;
using BBWM.FormIO.Interfaces.FormViewInterfaces;
using BBWM.FormIO.Models.FormViewModels;
using BBWM.FormIO.Services.FormVersioningServices;
using BBWM.FormIO.Services.FormViewServices;
using Microsoft.AspNetCore.Identity;

namespace BBWM.FormIO;

public class ModuleLinkage :
    IServicesModuleLinkage,
    IDependenciesModuleLinkage,
    IConfigureModuleLinkage,
    IInitialDataModuleLinkage,
    IDbModelCreateModuleLinkage
{
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
    }

    public void ConfigureModule(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        app.RegisterSection<FormioSettings>("FormioSettings");

        // Register Forms JSON tables as source for Reporting tables
        QueryableTablesProvidersRegister.RegisterQueryableTablesProvider(typeof(IFormsQueryableTablesProvider));
    }

    public void RegisterDependencies(ContainerBuilder builder)
    {
        builder.RegisterService<IRouteRolesModule, RouteRolesModule>();
        builder.RegisterService<IFormIODefinitionService, FormIODefinitionService>();
        builder.RegisterService<IFormIODataService, FormIODataService>();
        builder.RegisterService<IFormIOParameterListService, FormIOParameterListService>();
        builder.RegisterService<IFormIOFileService, FormIOFileService>();
        builder.RegisterService<IFormioIORevisionService, FormIORevisionService>();
        builder.RegisterService<IFormIOMultiUserFormDefinitionService, FormIOMultiUserFormDefinitionService>();
        builder.RegisterService<IFormIOMultiUserFormStageService, FormIOMultiUserFormStageService>();
        builder.RegisterService<IFormIOMultiUserFormPermissionsService, FormIOMultiUserFormPermissionsService>();
        builder.RegisterService<IFormIORequestService, FormIORequestService>();
        builder.RegisterService<IFormIOCategoryService, FormIOCategoryService>();

        // Form view services.
        builder.RegisterService<IFormFieldService, FormFieldService>();
        builder.RegisterService<IFormRevisionGridService, FormRevisionGridService>();
        builder.RegisterService<IFormViewDeclarationService, FormViewDeclarationService>();
        builder.RegisterService<IFormGridViewDeclarationService, FormGridViewDeclarationService>();
        builder.RegisterService<IFormViewHelperService, FormViewHelperService>();
        builder.RegisterService<IFormViewService, FormViewService>();
        builder.RegisterService<IFormDataVersioningService, FormDataVersioningService>();
    }

    public void OnModelCreating(ModelBuilder builder)
    {
        // Registering models of the module for the main project's DB context
        builder.Entity<FormDefinition>().Property(definition => definition.ViewName).HasMaxLength(64);

        builder.Entity<FormData>()
            .HasOne(formData => formData.Organization).WithMany()
            .HasForeignKey(formData => formData.OrganizationId).IsRequired(false);

        builder.Entity<FormData>()
            .HasOne(formData => formData.Survey)
            .WithMany(formSurvey => formSurvey.SurveyFormDataInstances)
            .HasForeignKey(formData => formData.SurveyId).IsRequired(false);

        builder.Entity<FormDataDraft>();
        builder.Entity<FormPrinting>();
        builder.Entity<FormParameterList>();
        builder.Entity<FormRevision>();

        builder.Entity<MultiUserFormStage>()
            .HasMany(multiUserFormStage => multiUserFormStage.Groups).WithMany();

        builder.Entity<MultiUserFormStagePermissions>();

        builder.Entity<FormSurvey>()
            .HasMany(formSurvey => formSurvey.SurveyFormDataInstances)
            .WithOne(formData => formData.Survey).OnDelete(DeleteBehavior.SetNull);

        builder.Entity<FormRequest>();
        builder.Entity<FormCategory>();
        builder.Entity<MultiUserFormAssociations>();

        // Definition of the many-to-many without navigational properties on the Organization's model
        builder.Entity<FormDefinitionOrganization>()
            .HasKey(formDefinitionOrganization => new { formDefinitionOrganization.FormDefinitionId, formDefinitionOrganization.OrganizationId });

        builder.Entity<FormDefinitionOrganization>()
            .HasOne(formDefinitionOrganization => formDefinitionOrganization.FormDefinition)
            .WithMany(formDefinition => formDefinition.FormDefinitionOrganizations)
            .HasForeignKey(formDefinitionOrganization => formDefinitionOrganization.FormDefinitionId);

        builder.Entity<FormDefinitionOrganization>()
            .HasOne(formDefinitionOrganization => formDefinitionOrganization.Organization).WithMany()
            .HasForeignKey(formDefinitionOrganization => formDefinitionOrganization.OrganizationId);

        // Definition of the many-to-many without navigational properties on the Organization's model
        builder.Entity<MultiUserFormDefinitionOrganization>()
            .HasKey(formDefinitionOrganization => new { formDefinitionOrganization.MultiUserFormDefinitionId, formDefinitionOrganization.OrganizationId });

        builder.Entity<MultiUserFormDefinitionOrganization>()
            .HasOne(formDefinitionOrganization => formDefinitionOrganization.MultiUserFormDefinition)
            .WithMany(multiUserFormDefinition => multiUserFormDefinition.MultiUserFormDefinitionOrganizations)
            .HasForeignKey(formDefinitionOrganization => formDefinitionOrganization.MultiUserFormDefinitionId);

        builder.Entity<MultiUserFormDefinitionOrganization>()
            .HasOne(multiUserFormDefinitionOrganization => multiUserFormDefinitionOrganization.Organization).WithMany()
            .HasForeignKey(multiUserFormDefinitionOrganization => multiUserFormDefinitionOrganization.OrganizationId);

        // Register view related models.
        builder.Entity<FormRevisionGrid>()
            .HasOne(formRevisionGrid => formRevisionGrid.FormDefinition).WithMany();

        builder.Entity<FormRevisionGrid>()
            .HasOne(formRevisionGrid => formRevisionGrid.ParentFormRevisionGrid)
            .WithMany().OnDelete(DeleteBehavior.NoAction);

        // Registering models configurations of the module for the main project's DB context
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public async Task EnsureInitialData(IServiceScope serviceScope, bool includingOnceSeededData)
    {
        var context = serviceScope.ServiceProvider.GetService<IDbContext>();

        #region Formio External User Role

        var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<Role>>();
        if (roleManager != null)
        {
            var role = await roleManager.FindByNameAsync("FormioExternalUser");
            if (role == null)
            {
                role = new Role("FormioExternalUser") { Id = Guid.NewGuid().ToString() };
                roleManager.CreateAsync(role).Wait();
            }
        }

        #endregion

        #region MUF Sequence Step initialization for old forms

        // Fix MultiUserFormAssociations with no value on TotalSteps
        var mufAssocsNoTotal = await context.Set<MultiUserFormAssociations>().Include(x => x.MultiUserFormAssociationLinks)
            .ThenInclude(x => x.MultiUserFormStage).Where(x => x.TotalSequenceSteps == 0).ToListAsync();
        if (mufAssocsNoTotal.Any())
        {
            foreach (var item in mufAssocsNoTotal)
            {
                var total = (await context.Set<MultiUserFormAssociationLinks>().Include(x => x.MultiUserFormStage)
                        .Where(x => x.MultiUserFormAssociationsId == item.Id).OrderByDescending(x => x.MultiUserFormStage.SequenceStepIndex)
                        .FirstOrDefaultAsync())
                    ?.MultiUserFormStage.SequenceStepIndex ?? -1;
                if (total > 0)
                {
                    item.TotalSequenceSteps = total;
                }

                context.Set<MultiUserFormAssociations>().Update(item);
            }

            await context.SaveChangesAsync();
        }

        var mufAssocsCompleted = await context.Set<MultiUserFormAssociations>().Include(x => x.MultiUserFormDefinition)
            .Include(x => x.MultiUserFormAssociationLinks).ThenInclude(x => x.MultiUserFormStage)
            .Where(x => x.MultiUserFormAssociationLinks.All(y => y.IsFilled)).ToListAsync();
        if (mufAssocsCompleted.Any())
        {
            foreach (var item in mufAssocsCompleted)
            {
                item.ActiveStepSequenceIndex = item.TotalSequenceSteps;
                context.Set<MultiUserFormAssociations>().Update(item);
            }

            await context.SaveChangesAsync();
        }


        var mufAssocs = await context.Set<MultiUserFormAssociations>().Include(x => x.MultiUserFormDefinition).Include(x => x.MultiUserFormAssociationLinks)
            .ThenInclude(x => x.MultiUserFormStage).Where(x => x.ActiveStepSequenceIndex == 0).ToListAsync();
        if (mufAssocs.Any())
        {
            foreach (var item in mufAssocs)
            {
                var activeStep =
                    item.MultiUserFormAssociationLinks.Where(x => !x.IsFilled).OrderBy(x => x.MultiUserFormStage.SequenceStepIndex).FirstOrDefault()
                        ?.MultiUserFormStage?.SequenceStepIndex ?? item.TotalSequenceSteps;
                item.ActiveStepSequenceIndex = activeStep;
                context.Set<MultiUserFormAssociations>().Update(item);
            }

            await context.SaveChangesAsync();
        }

        #endregion

        #region Formio Default Category

        // Ensure at least one category exists
        var anyCat = await context.Set<FormCategory>().AnyAsync();
        FormCategory? cat = null;
        if (!anyCat)
        {
            cat = (await context.Set<FormCategory>().AddAsync(new FormCategory { Name = "Default Category" })).Entity;
            context.SaveChanges();
        }
        else
        {
            cat = await context.Set<FormCategory>().FirstAsync();
        }

        // Ensure any pre-existing forms are associated to a category (First One by default)
        if (cat != null)
        {
            await context.Set<FormDefinition>().Where(x => x.FormCategoryId == null).UpdateFromQueryAsync(x => new FormDefinition { FormCategoryId = cat.Id });
        }

        #endregion

        #region Set default values

        // Ensure ByRequestOnly is false for all old forms
        await context.Set<FormDefinition>().Where(x => x.ByRequestOnly == null).UpdateFromQueryAsync(x => new FormDefinition { ByRequestOnly = false });

        // Any default data to be set
        var service = serviceScope.ServiceProvider.GetService<ISettingsService>();
        // By default Formio is not active
        var appSettings = new[]
        {
            new SettingsDTO
            {
                Value = service.GetSettingsSection<FormioSettings>() ??
                        new FormioSettings
                        {
                            IsFormIOActive = true
                        }
            }
        };
        await service.Save(appSettings);

        #endregion

        return;
    }
}