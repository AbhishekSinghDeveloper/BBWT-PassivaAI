using AutoMapper;

using BBWM.Core.Data;
using BBWM.Core.Data.Attributes;
using BBWM.Demo.Northwind.DTO;

using System.ComponentModel.DataAnnotations.Schema;

namespace BBWM.Demo.Northwind.Model;

/// <summary>
/// Customer Order
/// </summary>
[Table("Orders")]
[AllowDeleteAll]
public class Order : IAuditableEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Order Date
    /// </summary>
    public DateTime? OrderDate { get; set; }

    /// <summary>
    /// Required Date
    /// </summary>
    public DateTime? RequiredDate { get; set; }

    /// <summary>
    /// Shipped Date
    /// </summary>
    public DateTime? ShippedDate { get; set; }

    /// <summary>
    /// Is Paid
    /// </summary>
    public bool IsPaid { get; set; }


    public int? CustomerId { get; set; }

    /// <summary>
    /// Customer
    /// </summary>
    public Customer Customer { get; set; }

    public int? EmployeeId { get; set; }

    /// <summary>
    /// Employee
    /// </summary>
    public Employee Employee { get; set; }

    /// <summary>
    /// List of order details
    /// </summary>
    public IList<OrderDetails> OrderDetails { get; set; }


    public static void RegisterMap(IMapperConfigurationExpression c)
    {
        c.CreateMap<Order, OrderDTO>()
            .ForMember(d => d.HasResellerItems, r => r.MapFrom(s => s.OrderDetails.Any(x => x.IsReseller)))
            .ReverseMap();
    }
}
