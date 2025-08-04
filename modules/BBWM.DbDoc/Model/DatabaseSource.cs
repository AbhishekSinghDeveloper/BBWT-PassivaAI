using BBWM.Core.Data;

namespace BBWM.DbDoc.Model;

public class DatabaseSource : IAuditableEntity<Guid>
{
    public Guid Id { get; set; }

    /// <summary>
    /// Identifier of context object (e.g. DB context class) in the application, assosiated with the database source.
    /// </summary>
    public string? ContextId { get; set; }

    /// <summary>
    /// SchemaCode is a prefix to identify unique table and column IDs within DbDoc system.
    /// </summary>
    public string SchemaCode { get; set; }

    public DatabaseType DatabaseType { get; set; }

    public string ConnectionString { get; set; }

    public Folder Folder { get; set; }
}