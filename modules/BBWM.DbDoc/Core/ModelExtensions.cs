using Microsoft.EntityFrameworkCore.Metadata;

namespace BBWM.DbDoc.Core;

public static class ModelExtensions
{
    public static IEnumerable<IEntityType> GetEntityTypesWithPrimaryKey(this IModel model)
    {
        return model.GetEntityTypes().Where(entityType => entityType.FindPrimaryKey() is not null);
    }
}
