using BBWM.Core.Services;
using BBWM.Demo.Northwind.DTO;
using BBWM.Demo.Northwind.Model;
using System.ComponentModel.DataAnnotations;

namespace BBWM.Demo.Northwind.Services;

public interface ICustomerService : IEntityValidate<CustomerDTO>
{
    string[] GetAllCompanies();
}

public class CustomerService : ICustomerService
{
    private readonly IDemoDataContext _context;

    public CustomerService(IDemoDataContext context) => _context = context;

    public string[] GetAllCompanies() => _context.Set<Customer>().Select(x => x.CompanyName).Distinct().ToArray();

    public void Validate(CustomerDTO dto, CancellationToken ct = default)
    {
        var validationErrors = new List<string>();

        if (string.IsNullOrWhiteSpace(dto.Code))
            validationErrors.Add(($"The '{nameof(dto.Code)}' should not be empty."));

        if (validationErrors.Any())
            throw new ValidationException(string.Join(" ", validationErrors));
    }
}
