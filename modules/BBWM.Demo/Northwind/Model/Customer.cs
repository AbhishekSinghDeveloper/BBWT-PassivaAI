using AutoMapper;

using BBWM.Core.Data;
using BBWM.Core.Data.Attributes;
using BBWM.Demo.Northwind.DTO;

using System.ComponentModel.DataAnnotations.Schema;

namespace BBWM.Demo.Northwind.Model;

/// <summary>
/// Demo customer
/// </summary>
[Table("Customers")]
[AllowDeleteAll]
public class Customer : IAuditableEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Customer code
    /// </summary>
    public string Code { get; set; }

    /// <summary>
    /// Customer company name
    /// </summary>
    public string CompanyName { get; set; }


    /// <summary>
    /// Customer orders
    /// </summary>
    public IList<Order> Orders { get; set; }


    public static void RegisterMap(IMapperConfigurationExpression c)
    {
        c.CreateMap<Customer, CustomerDTO>()
            .ForMember(x => x.Orders, y => y.Ignore())
            .ReverseMap();

        c.CreateMap<Customer, SearchCustomerDTO>();
    }
}
