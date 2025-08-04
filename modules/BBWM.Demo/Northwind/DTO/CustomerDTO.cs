using BBWM.Core.DTO;

namespace BBWM.Demo.Northwind.DTO;

/// <summary>
/// Demo customer
/// </summary>
public class CustomerDTO : IDTO
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
    public IList<OrderDTO> Orders { get; set; }
}
