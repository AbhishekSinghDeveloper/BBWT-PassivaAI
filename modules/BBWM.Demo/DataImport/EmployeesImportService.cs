using AutoMapper;

using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.DTO;
using BBWM.DataProcessing.Services;
using BBWM.Demo.Northwind.DTO;
using BBWM.Demo.Northwind.Model;

using Microsoft.AspNetCore.SignalR;

namespace BBWM.Demo.DataImport;

/// <summary>
/// The demo service which shows how we can import employees by IDataImportService functionality.
/// </summary>
public interface IEmployeesImportService : IImportService
{
}

public class EmployeesImportService : BaseImportService<EmployeeDTO>, IEmployeesImportService
{
    private static readonly string[] AvailableJobRoles = { "Admin", "Manager", "Supervisor" };
    private readonly IDemoDataContext _context;

    public EmployeesImportService(
        IDataImportHelper dataImportHelper,
        IDemoDataContext context,
        IMapper mapper,
        IHubContext<DataImportHub> hubContext)
        : base(dataImportHelper, hubContext, mapper)
        => _context = context;

    /// <summary>
    /// Return a custom <see cref="CustomValidationHandler"/> instance by the Lookup list item.
    /// </summary>
    /// <returns><see cref="CustomValidationHandler"/> instance.</returns>
    protected override CustomValidationHandler GetCustomValidator(CellDataTypeInfoDTO typeInfo) =>
        string.Equals(typeInfo.CustomValidation, "JobRole", StringComparison.CurrentCultureIgnoreCase)
            ? JobRoleHandler()
            : null;

    /// <summary>
    /// Returns a <see cref="CustomValidationHandler"/> instance that validates for a Job Role.
    /// </summary>
    /// <returns><see cref="CustomValidationHandler"/> instance.</returns>
    private static CustomValidationHandler JobRoleHandler() =>
        validationResult =>
        {
            var job = Convert.ToString(validationResult.Value);

            if (!AvailableJobRoles.Contains(job, StringComparer.CurrentCultureIgnoreCase))
            {
                validationResult.ErrorMessage = "Value is not a JobRole";
            }
        };

    protected override async Task SaveImportedEntities(IEnumerable<EmployeeDTO> list, CancellationToken ct)
        => await _context.SaveChangesAsync(ct);

    protected override async Task OnEntityImport(EmployeeDTO entity, CancellationToken ct)
        => await _context.Employees.AddAsync(Mapper.Map<Employee>(entity), ct);
}
