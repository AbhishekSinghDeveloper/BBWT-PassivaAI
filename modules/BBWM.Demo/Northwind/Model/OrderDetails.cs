using BBWM.Core.Data;

using System.ComponentModel.DataAnnotations.Schema;

namespace BBWM.Demo.Northwind.Model;

/// <summary>
/// Order Details
/// </summary>
[Table("OrderDetails")]
public class OrderDetails : IAuditableEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Quantity
    /// </summary>
    public decimal Quantity { get; set; }

    /// <summary>
    /// Price
    /// </summary>
    public decimal Price { get; set; }

    /// <summary>
    /// Is Reseller
    /// </summary>
    public bool IsReseller { get; set; }


    /// <summary>
    /// Product Id
    /// </summary>
    public int ProductId { get; set; }

    /// <summary>
    /// Order product
    /// </summary>
    public Product Product { get; set; }

    /// <summary>
    /// Order Id
    /// </summary>
    public int OrderId { get; set; }

    public Order Order { get; set; }
}
