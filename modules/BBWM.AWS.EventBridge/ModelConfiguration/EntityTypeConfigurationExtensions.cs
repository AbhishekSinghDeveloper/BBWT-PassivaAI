using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using System.Linq.Expressions;
using System.Text.Json;

namespace BBWM.AWS.EventBridge.ModelConfiguration;

internal static class EntityTypeConfigurationExtensions
{
    private static readonly JsonSerializerOptions paramsOptions =
        new JsonSerializerOptions
        {
            IgnoreNullValues = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

    public static void HasListToJsonConversion<T>(
        this PropertyBuilder<List<T>> property, Expression<Func<List<T>, List<T>>> deepCopy)
    {
        property.HasConversion(
                items =>
                    JsonSerializer.Serialize(items ?? new List<T>(), paramsOptions),
                items =>
                    JsonSerializer.Deserialize<List<T>>(items, paramsOptions))
            .Metadata
            .SetValueComparer(
                new ValueComparer<List<T>>(
                    (c1, c2) => c1.SequenceEqual(c2),
                    @params => @params.Aggregate(0, (acc, param) => HashCode.Combine(acc, param.GetHashCode())),
                    deepCopy));
    }
}
