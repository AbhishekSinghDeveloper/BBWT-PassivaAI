using AutoMapper;

using BBWM.Core.Data;
using BBWM.Core.Data.Attributes;
using BBWM.Demo.Northwind.DTO;

using System.ComponentModel.DataAnnotations.Schema;

namespace BBWM.Demo.Northwind.Model;

/// <summary>
/// Demo employee entity
/// </summary>
[Table("Employees")]
[AllowDeleteAll]
public class Employee : IAuditableEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Age
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Phone number
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Registration date
    /// </summary>
    public DateTime? RegistrationDate { get; set; }

    /// <summary>
    /// Job role
    /// </summary>
    public string JobRole { get; set; }

    /// <summary>
    /// Employee orders
    /// </summary>
    public IList<Order> Orders { get; set; }

    public static void RegisterMap(IMapperConfigurationExpression c)
    {
        c.CreateMap<Employee, EmployeeDTO>().ReverseMap();
    }
}
