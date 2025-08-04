using BBF.Reporting.Core.DTO;

namespace BBF.Reporting.Core.Interfaces;

public interface IVariablesService
{
    Task<IEnumerable<VariableDTO>> GetAll(CancellationToken ct = default);
}