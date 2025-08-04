using BBWM.Core.DTO;

namespace BBF.Reporting.TableSet.DTO;

public class TableSetTableDTO : IDTO<string>
{
    public string Id { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? ParentTableId { get; set; }
    public string FolderId { get; set; } = null!;
    public string TableAlias { get; set; } = null!;
    public string SourceCode { get; set; } = null!;
    public IEnumerable<TableSetTableDTO> Children { get; set; } = new List<TableSetTableDTO>();
    public IEnumerable<TableSetColumnDTO> Columns { get; set; } = new List<TableSetColumnDTO>();
}