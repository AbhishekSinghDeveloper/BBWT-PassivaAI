using Microsoft.EntityFrameworkCore;
using System.Reflection;

namespace BBWM.Core.ModelHashing;

public static class DbContextExtensions
{
    public static Dictionary<PropertyInfo, Type> FindKeys(this DbContext context, Type type)
    {
        var entityType = context.Model.FindEntityType(type);
        if (entityType is not null)
        {
            var foreignKeys = entityType.GetForeignKeys()
                .SelectMany(k =>
                    k.Properties
                        .Where(p => (p.ClrType == typeof(int) || p.ClrType == typeof(int?)) && p.PropertyInfo is not null)
                        .Select(p => new
                        {
                            Property = p.PropertyInfo,
                            Principal = k.PrincipalEntityType.ClrType
                        })
                )
                .ToArray();

            var primaryKeys = entityType.GetKeys()
                .SelectMany(k =>
                    k.Properties
                        .Where(p => (p.ClrType == typeof(int) || p.ClrType == typeof(int?)) && p.PropertyInfo is not null)
                        .Select(p => new
                        {
                            Property = p.PropertyInfo,
                            Principal = type
                        })
                )
                .ToArray();

            return foreignKeys
                .Concat(primaryKeys)
                .GroupBy(p => p.Property)
                .ToDictionary(g => g.Key, g => g.FirstOrDefault().Principal);
        }

        return null;
    }
}
