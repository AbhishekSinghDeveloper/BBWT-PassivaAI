using BBWM.Core.DTO;

namespace BBWM.Core.Audit;

/// <summary>
/// Audit definition
/// </summary>
public class ChangeLogDTO : IDTO
{
    public int Id { get; set; }

    public string State { get; set; }

    public DateTime DateTime { get; set; }

    public string EntityName { get; set; }

    public string TableName { get; set; }

    public string EntityId { get; set; }

    public string UserName { get; set; }

    public IList<ChangeLogItemDTO> ChangeLogItems { get; set; }
}
