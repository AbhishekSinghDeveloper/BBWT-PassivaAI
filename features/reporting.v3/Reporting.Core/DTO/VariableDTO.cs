using BBWM.Core.DTO;

namespace BBF.Reporting.Core.DTO;

public class VariableDTO : IDTO
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
}