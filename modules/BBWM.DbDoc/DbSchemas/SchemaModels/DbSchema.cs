using BBWM.DbDoc.Model;

namespace BBWM.DbDoc.DbSchemas.SchemaModels;

public class DbSchema
{
    public DatabaseSource DatabaseSource { get; set; }
    /// <summary>
    /// SchemaCode is a prefix that identify unique table and column IDs within DbDoc system.
    /// </summary>
    public string SchemaCode { get; set; }
    public string DatabaseName { get; set; }
    public IReadOnlyDictionary<string, DbSchemaTable> Tables { get; set; }
    public IReadOnlyDictionary<string, DbSchemaColumn> Columns { get; set; }
    public IEnumerable<DbSchemaColumn> GetTableColumns(string tableId) =>
        Columns.Values.Where(x => x.TableId == tableId);
}
