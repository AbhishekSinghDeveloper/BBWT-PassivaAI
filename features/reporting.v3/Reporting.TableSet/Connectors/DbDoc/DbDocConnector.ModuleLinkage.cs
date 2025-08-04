using BBWM.Core.ModuleLinker;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace BBF.Reporting.TableSet.Connectors.DbDoc;

public class DbDocConnectorModuleLinkage : IConfigureModuleLinkage
{
    public void ConfigureModule(IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.GetService<IServiceScopeFactory>()!.CreateScope();
        BBWM.DbDoc.DbDocFolderOwnersRegister.RegisterFolderOwnerType(DbDocConnector.DbDocFolderOwnerName, true);
    }
}
