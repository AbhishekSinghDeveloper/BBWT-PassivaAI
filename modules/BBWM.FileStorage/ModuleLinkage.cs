using BBWM.Core.ModuleLinker;
using Microsoft.EntityFrameworkCore;

namespace BBWM.FileStorage;

public class ModuleLinkage : IDbModelCreateModuleLinkage
{
    public void OnModelCreating(ModelBuilder builder)
    {
        // Registering models of the module for the main project's DB context
        builder.Entity<FileDetails>()
            .ToTable("FilesDetails");
    }
}