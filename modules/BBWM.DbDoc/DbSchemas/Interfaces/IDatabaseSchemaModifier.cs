using BBWM.Core.Data.DatabaseSchema;

namespace BBWM.DbDoc.DbSchemas.Interfaces;

public interface IDatabaseSchemaModifier
{
    DatabaseSchema ModifySchema(DatabaseSchema schema);
}