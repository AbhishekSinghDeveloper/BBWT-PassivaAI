using BBWM.Core.Data;

namespace BBWM.DbDoc.DbSchemas.DTO;

public class DatabaseSourceRegisterRequest
{
    public string ContextId { get; set; }
    public string SchemaCode { get; set; }
    public DatabaseType DatabaseType { get; set; }
    public string ConnectionString { get; set; }
}
