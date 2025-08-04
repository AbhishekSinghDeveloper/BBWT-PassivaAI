using BBWM.Core.DTO;

using System.Text.Json;

namespace BBWM.Demo.Northwind.DTO;

/// <summary>
/// Customer Order
/// </summary>
public class OrderDTO : IDTO
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Customer ID
    /// </summary>
    public string CustomerCode { get; set; }

    /// <summary>
    /// Customer Company Name
    /// </summary>
    public string CustomerCompanyName { get; set; }

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

    /// <summary>
    /// Has Reseller Items
    /// </summary>
    public bool HasResellerItems { get; set; }

    public int? CustomerId { get; set; }

    /// <summary>
    /// Customer
    /// </summary>
    public CustomerDTO Customer { get; set; }

    public int? EmployeeId { get; set; }

    /// <summary>
    /// Employee
    /// </summary>
    public EmployeeDTO Employee { get; set; }

    public List<OrderDetailsDTO> OrderDetails { get; set; } = new List<OrderDetailsDTO>();

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}