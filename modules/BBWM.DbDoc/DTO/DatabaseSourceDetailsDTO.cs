using BBWM.Core.Data;

namespace BBWM.DbDoc.DTO;

public class DatabaseSourceDetailsDTO
{
    public Guid Id { get; set; }

    public string ContextId { get; set; }

    public string SchemaCode { get; set; }

    public DatabaseType DatabaseType { get; set; }

    public string DatabaseName { get; set; }
}
