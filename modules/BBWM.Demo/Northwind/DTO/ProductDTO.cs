using BBWM.Core.DTO;

namespace BBWM.Demo.Northwind.DTO;

/// <summary>
/// Product
/// </summary>
public class ProductDTO : IDTO
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Title
    /// </summary>
    public string Title { get; set; }
}
