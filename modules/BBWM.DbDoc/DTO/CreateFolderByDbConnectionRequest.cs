using BBWM.Core.Data;

namespace BBWM.DbDoc.DTO;

public class CreateFolderByDbConnectionRequest
{
    public string FolderName { get; set; }
    public string FolderDescription { get; set; }
    public string ContextId { get; set; }
    public string ConnectionString { get; set; }
    public DatabaseType DatabaseType { get; set; }
}
