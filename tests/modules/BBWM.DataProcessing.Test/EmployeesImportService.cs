using AutoMapper;

using BBWM.Core.Data;
using BBWM.DataProcessing.Classes;
using BBWM.DataProcessing.DTO;
using BBWM.DataProcessing.Services;
using BBWM.Demo.Northwind.DTO;
using BBWM.Demo.Northwind.Model;
using Microsoft.AspNetCore.SignalR;

namespace BBWT.Tests.modules.BBWM.DataProcessing.Test;

/// <summary>
/// The demo service which shows how we can import employees by IDataImportService functionality
/// </summary>
public interface IEmployeesImportService : IImportService
{
}

public class EmployeesImportService : BaseImportService<EmployeeDTO>, IEmployeesImportService
{
    private static readonly string[] AvailableJobRoles = { "Admin", "Manager", "Supervisor" };
    private readonly IDbContext dbContext;

    public EmployeesImportService(
        IDataImportHelper dataImportHelper,
        IDbContext dbContext,
        IMapper mapper,
        IHubContext<DataImportHub> hubContext)
        : base(dataImportHelper, hubContext, mapper)
    {
        this.dbContext = dbContext;
    }

    /// <summary>
    /// Return a custom <see cref="CustomValidationHandler"/> instance by the Lookup list item
    /// </summary>
    /// <param name="item">Lookup list item</param>
    /// <returns><see cref="CustomValidationHandler"/> instance</returns>
    protected override CustomValidationHandler GetCustomValidator(CellDataTypeInfoDTO typeInfo) =>
        string.Equals(typeInfo.CustomValidation, "JobRole", StringComparison.CurrentCultureIgnoreCase)
            ? JobRoleHandler()
            : null;

    protected override async Task SaveImportedEntities(IEnumerable<EmployeeDTO> list, CancellationToken ct)
    {
        var entities = Mapper.Map<IEnumerable<Employee>>(list);
        await dbContext.Set<Employee>().AddRangeAsync(entities, ct);
        await dbContext.SaveChangesAsync();
    }

    protected override Task OnEntityImport(EmployeeDTO entity, CancellationToken ct)
        => Task.CompletedTask;
    /// <summary>
    /// Returns a <see cref="CustomValidationHandler"/> instance that validates for a Job Role
    /// </summary>
    /// <returns><see cref="CustomValidationHandler"/> instance</returns>
    private static CustomValidationHandler JobRoleHandler() =>
        validationResult =>
        {
            var job = Convert.ToString(validationResult.Value);

            if (!AvailableJobRoles.Contains(job, StringComparer.CurrentCultureIgnoreCase))
            {
                validationResult.ErrorMessage = "Value is not a JobRole";
            }
        };
}
