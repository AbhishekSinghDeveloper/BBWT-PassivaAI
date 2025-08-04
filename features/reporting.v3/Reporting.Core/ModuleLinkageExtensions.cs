using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BBF.Reporting.Core;

public static class ModuleLinkageExtensions
{
    public static EntityTypeBuilder<T> RegisterReportingTable<T>(this ModelBuilder builder) where T : class
        => builder.Entity<T>().ToTable("Rb" + typeof(T).Name);
}