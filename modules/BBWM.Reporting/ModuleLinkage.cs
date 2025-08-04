using Autofac;

using BBWM.Core.Autofac;
using BBWM.Core.Data;
using BBWM.Core.Membership;
using BBWM.Core.Membership.Model;
using BBWM.Core.Membership.Utils;
using BBWM.Core.ModelHashing;
using BBWM.Core.ModuleLinker;
using BBWM.Menu;
using BBWM.Menu.DTO;
using BBWM.Reporting.DTO;
using BBWM.Reporting.Enums;
using BBWM.Reporting.Model;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using MySqlConnector;

using SqlKata.Compilers;
using SqlKata.Execution;

using System.Reflection;

namespace BBWM.Reporting;

public class ModuleLinkage :
    IServicesModuleLinkage,
    IDependenciesModuleLinkage,
    IMenuModuleLinkage,
    IInitialDataModuleLinkage,
    IConfigureModuleLinkage,
    IDbModelCreateModuleLinkage
{
    public const string DbDocFolderOwnerName = "Reporting v2";

    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        var dbConfig = configuration.GetSection("DatabaseConnectionSettings").Get<DatabaseConnectionSettings>();
        if (dbConfig.DatabaseType == DatabaseType.MySql)
        {
            services.AddSingleton<QueryFactory>((serviceProvider) =>
            {
                var connection = new MySqlConnection(configuration.GetConnectionString("MySqlConnection"));
                return new QueryFactory(connection, new MySqlCompiler());
            });
        }
        else
        {
            services.AddSingleton<QueryFactory>((serviceProvider) =>
            {
                var connection = new SqlConnection(configuration.GetConnectionString("DefaultConnection"));
                return new QueryFactory(connection, new SqlServerCompiler());

            });
        }
    }

    public void ConfigureModule(IApplicationBuilder app)
    {
        using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
        var modelHashingService = serviceScope.ServiceProvider.GetService<IModelHashingService>();
        modelHashingService.ManualPropertyHashing<SectionViewFilterDTO, QueryFilter>(e => e.QueryFilterId);
        modelHashingService.ManualPropertyHashing<SectionViewFilterDTO, FilterControl>(e => e.FilterControlId);
    }

    public void RegisterDependencies(ContainerBuilder builder)
    {
        builder.RegisterService<IRouteRolesModule, RouteRolesModule>();

        DbDoc.DbDocFolderOwnersRegister.RegisterFolderOwnerType(DbDocFolderOwnerName);
    }

    public void OnModelCreating(ModelBuilder builder)
    {
        // Registering models of the module for the main project's DB context
        builder.Entity<Report>();
        builder.Entity<ReportRole>();
        builder.Entity<ReportPermission>();
        builder.Entity<Query>();
        builder.Entity<NamedQuery>();
        builder.Entity<View>();
        builder.Entity<Section>();
        builder.Entity<QueryRule>();
        builder.Entity<QueryRuleType>();
        builder.Entity<QueryTable>();
        builder.Entity<QueryTableColumn>();
        builder.Entity<QueryFilterSet>();
        builder.Entity<QueryFilter>();
        builder.Entity<GridView>();
        builder.Entity<GridViewColumn>();
        builder.Entity<FilterControl>();
        builder.Entity<QueryFilterBinding>();
        builder.Entity<QueryTableJoin>();

        // Registering models configurations of the module for the main project's DB context
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public void CreateInitialMenuItems(List<MenuDTO> menu, MenuLinkageRootMenus rootMenus) =>
        rootMenus.OperationalAdmin.Children.Add(new MenuDTO(Routes.Reports, "view_list"));

    public async Task EnsureInitialData(IServiceScope serviceScope, bool includingOnceSeededData)
    {
        var context = serviceScope.ServiceProvider.GetService<IDbContext>();
        var roleManager = serviceScope.ServiceProvider.GetService<RoleManager<Role>>();

        await CreateInitialRoles(roleManager);
        await SeedQueryRules(context);
    }


    private async Task CreateInitialRoles(RoleManager<Role> roleManager)
    {
        var roleNames = RolesExtractor.GetRolesNamesOfClass(typeof(Roles));

        foreach (var roleName in roleNames)
        {
            var role = await roleManager.FindByNameAsync(roleName);
            if (role is null)
            {
                role = new Role(roleName) { Id = Guid.NewGuid().ToString() };
                await roleManager.CreateAsync(role);
            }
        }
    }

    private async Task SeedQueryRules(IDbContext context)
    {
        if (!context.Set<QueryRule>().Any())
        {
            context.Set<QueryRule>().AddRange(new[]
            {
                new QueryRule
                {
                    Code = QueryRuleCode.Equals,
                    Name = nameof(QueryRuleCode.Equals),
                    RuleTypes = new []
                    {
                        new QueryRuleType { Type = QueryRuleDataType.String },
                        new QueryRuleType { Type = QueryRuleDataType.Numeric },
                        new QueryRuleType { Type = QueryRuleDataType.Datetime },
                        new QueryRuleType { Type = QueryRuleDataType.Boolean }
                    }
                },
                new QueryRule
                {
                    Code = QueryRuleCode.NotEquals,
                    Name = nameof(QueryRuleCode.NotEquals),
                    RuleTypes = new []
                    {
                        new QueryRuleType { Type = QueryRuleDataType.String },
                        new QueryRuleType { Type = QueryRuleDataType.Numeric },
                        new QueryRuleType { Type = QueryRuleDataType.Datetime },
                        new QueryRuleType { Type = QueryRuleDataType.Boolean }
                    }
                },
                new QueryRule
                {
                    Code = QueryRuleCode.Contains,
                    Name = nameof(QueryRuleCode.Contains),
                    RuleTypes = new []
                    {
                        new QueryRuleType { Type = QueryRuleDataType.String }
                    }
                },
                new QueryRule
                {
                    Code = QueryRuleCode.NotContains,
                    Name = nameof(QueryRuleCode.NotContains),
                    RuleTypes = new []
                    {
                        new QueryRuleType { Type = QueryRuleDataType.String }
                    }
                },
                new QueryRule
                {
                    Code = QueryRuleCode.StartsWith,
                    Name = nameof(QueryRuleCode.StartsWith),
                    RuleTypes = new []
                    {
                        new QueryRuleType { Type = QueryRuleDataType.String }
                    }
                },
                new QueryRule
                {
                    Code = QueryRuleCode.EndsWith,
                    Name = nameof(QueryRuleCode.EndsWith),
                    RuleTypes = new []
                    {
                        new QueryRuleType { Type = QueryRuleDataType.String }
                    }
                },
                new QueryRule
                {
                    Code = QueryRuleCode.More,
                    Name = nameof(QueryRuleCode.More),
                    RuleTypes = new []
                    {
                        new QueryRuleType { Type = QueryRuleDataType.Numeric },
                        new QueryRuleType { Type = QueryRuleDataType.Datetime }
                    }
                },
                new QueryRule
                {
                    Code = QueryRuleCode.MoreOrEqual,
                    Name = nameof(QueryRuleCode.MoreOrEqual),
                    RuleTypes = new []
                    {
                        new QueryRuleType { Type = QueryRuleDataType.Numeric },
                        new QueryRuleType { Type = QueryRuleDataType.Datetime }
                    }
                },
                new QueryRule
                {
                    Code = QueryRuleCode.Less,
                    Name = nameof(QueryRuleCode.Less),
                    RuleTypes = new []
                    {
                        new QueryRuleType { Type = QueryRuleDataType.Numeric },
                        new QueryRuleType { Type = QueryRuleDataType.Datetime }
                    }
                },
                new QueryRule
                {
                    Code = QueryRuleCode.LessOrEqual,
                    Name = nameof(QueryRuleCode.LessOrEqual),
                    RuleTypes = new []
                    {
                        new QueryRuleType { Type = QueryRuleDataType.Numeric },
                        new QueryRuleType { Type = QueryRuleDataType.Datetime }
                    }
                },
                new QueryRule
                {
                    Code = QueryRuleCode.Between,
                    Name = nameof(QueryRuleCode.Between),
                    RuleTypes = new []
                    {
                        new QueryRuleType { Type = QueryRuleDataType.Numeric },
                        new QueryRuleType { Type = QueryRuleDataType.Datetime }
                    }
                }
            });
            await context.SaveChangesAsync();
        }
    }
}
