using BBF.Reporting.Core.DbModel;
using BBF.Reporting.Core.DTO;
using BBF.Reporting.Core.Interfaces;
using BBWM.Core.Services;

namespace BBF.Reporting.Core.Services;

public class VariablesService : IVariablesService
{
    private readonly IDataService _dataService;

    public VariablesService(IDataService dataService)
        => _dataService = dataService;

    public Task<IEnumerable<VariableDTO>> GetAll(CancellationToken ct = default)
        => _dataService.GetAll<Variable, VariableDTO>(ct);
}