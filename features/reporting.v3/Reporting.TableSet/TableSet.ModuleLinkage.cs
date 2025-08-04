using System.Reflection;
using Autofac;
using BBF.Reporting.Core;
using BBF.Reporting.TableSet.Interfaces;
using BBF.Reporting.TableSet.Services;
using BBWM.Core.Autofac;
using BBWM.Core.ModuleLinker;
using Microsoft.EntityFrameworkCore;

namespace BBF.Reporting.TableSet;

public class QueryBuilderModuleLinkage :
    IDbModelCreateModuleLinkage,
    IDependenciesModuleLinkage
{
    public void OnModelCreating(ModelBuilder builder)
    {
        // Table set.
        var tsBuilder = builder.RegisterReportingTable<DbModel.TableSet>();
        tsBuilder.Property(x => x.FolderSourceCode).HasMaxLength(50);
        tsBuilder.Property(x => x.FolderId).HasMaxLength(50);

        // Registering models configurations of the module for the main project's DB context.
        builder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }

    public void RegisterDependencies(ContainerBuilder builder)
    {
        // Table set.
        builder.RegisterService<ITableSetService, TableSetService>();
    }
}