using BBWM.Core.DTO;

namespace BBWM.Demo.Northwind.DTO;

/// <summary>
/// Order Detail
/// </summary>
public class OrderDetailsDTO : IDTO
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Order ID
    /// </summary>
    public int OrderId { get; set; }

    /// <summary>
    /// Order product title
    /// </summary>
    public string ProductTitle { get; set; }

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

    public int? ProductId { get; set; }

    /// <summary>
    /// Product
    /// </summary>
    public ProductDTO Product { get; set; }
}
