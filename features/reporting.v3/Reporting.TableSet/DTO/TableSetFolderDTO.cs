using BBWM.Core.DTO;

namespace BBF.Reporting.TableSet.DTO;

public class TableSetFolderDTO : IDTO<string>
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string SourceCode { get; set; } = null!;
}