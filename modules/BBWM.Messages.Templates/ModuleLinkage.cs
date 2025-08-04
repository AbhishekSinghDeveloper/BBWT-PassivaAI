using BBWM.Core.ModuleLinker;
using Microsoft.EntityFrameworkCore;

namespace BBWM.Messages.Templates;

public class ModuleLinkage : IDbModelCreateModuleLinkage
{
    public void OnModelCreating(ModelBuilder builder)
    {
        // Registering models of the module for the main project's DB context
        builder.Entity<EmailTemplate>()
            .ToTable("EmailTemplates");
        builder.Entity<EmailTemplateParameter>()
            .ToTable("EmailTemplateParameters");
    }
}