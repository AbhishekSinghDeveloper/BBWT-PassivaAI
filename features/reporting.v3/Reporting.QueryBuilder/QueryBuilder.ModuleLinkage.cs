using Autofac;
using BBF.Reporting.Core;
using BBF.Reporting.Core.Interfaces;
using BBF.Reporting.QueryBuilder.DbModel;
using BBF.Reporting.QueryBuilder.Interfaces;
using BBF.Reporting.QueryBuilder.Services;
using BBF.Reporting.QueryBuilder.Interfaces.RbqMySqlQueryParser;
using BBF.Reporting.QueryBuilder.Services.RbqMySqlQueryParser;
using BBWM.Core.Autofac;
using BBWM.Core.ModuleLinker;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using BBWM.Core.Data;

namespace BBF.Reporting.QueryBuilder;

public class QueryBuilderModuleLinkage :
    IConfigureModuleLinkage,
    IDbModelCreateModuleLinkage,
    IDependenciesModuleLinkage
{
    public void ConfigureModule(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope();

        // Register query builder providers.
        var queryProviderFactory = scope.ServiceProvider.GetService<IQueryProviderFactory>();
        // Auto query builder
        const string autoQueryProviderType = AutoQuerySourceProvider.SourceType;
        queryProviderFactory!.RegisterQueryProvider<IAutoQuerySourceProvider>(autoQueryProviderType);
        queryProviderFactory.RegisterMetadataProvider<IAutoQuerySourceProvider>(autoQueryProviderType);
        // Raw query builder
        const string rqbQueryProviderType = RqbQuerySourceProvider.SourceType;
        queryProviderFactory.RegisterQueryProvider<IRqbQuerySourceProvider>(rqbQueryProviderType);
        queryProviderFactory.RegisterMetadataProvider<IRqbViewMetadataProvider>(rqbQueryProviderType);

        // Register sql query providers.
        var sqlQueryProviderFactory = scope.ServiceProvider.GetService<IRqbQueryProcessorFactory>();
        // MySQL provider.
        const DatabaseType mySql = RqbQueryProcessorMySql.DbType;
        sqlQueryProviderFactory!.RegisterSqlQueryProvider<IRqbQueryProcessorMySql>(mySql);
        // MSSQL provider.
        const DatabaseType msSql = RqbQueryProcessorMsSql.DbType;
        sqlQueryProviderFactory.RegisterSqlQueryProvider<IRqbQueryProcessorMsSql>(msSql);
    }

    public void OnModelCreating(ModelBuilder builder)
    {
        // SQL QB.
        var sBuilder = builder.RegisterReportingTable<SqlQuery>();
        sBuilder.Property(x => x.SqlCode).IsRequired();

        // Registering models configurations of the module for the main project's DB context.
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public void RegisterDependencies(ContainerBuilder builder)
    {
        // SQL query builder
        builder.RegisterService<IRqbQueryProcessorDefault, RqbQueryProcessorDefault>();
        builder.RegisterService<IRqbQueryProcessorMsSql, RqbQueryProcessorMsSql>();
        builder.RegisterService<IRqbQueryProcessorMySql, RqbQueryProcessorMySql>();
        builder.RegisterService<IRqbQuerySourceProvider, RqbQuerySourceProvider>();
        builder.RegisterService<IRqbQueryProcessorFactory, RqbQueryProcessorFactory>();
        builder.RegisterService<IRqbViewMetadataProvider, RqbViewMetadataProvider>();
        builder.RegisterService<IRqbService, RqbService>();
        builder.RegisterService<IRqbQueryParser, RqbQueryParser>();
        builder.RegisterService<IRqbQueryMySqlParser, RqbQueryMySqlParser>();
        builder.RegisterService<IRqbQueryMySqlParserHelper, RqbQueryMySqlParserHelper>();
        builder.RegisterService<IRqbQueryGraphService, RqbQueryGraphService>();

        // Auto query builder
        builder.RegisterService<IAutoQuerySourceProvider, AutoQuerySourceProvider>();
    }
}