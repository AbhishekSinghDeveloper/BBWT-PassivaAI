namespace BBWM.DbDoc.DbSchemas.SchemaModels;

public class DbSchemaTable
{
    public string TableId { get; set; }

    public string DbName { get; set; }

    public string QueryName { get; set; }

    public string Schema { get; set; }

    public string TableName { get; set; }

    public bool IsView { get; set; }
}
