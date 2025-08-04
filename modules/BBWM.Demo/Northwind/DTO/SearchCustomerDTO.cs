using BBWM.Core.DTO;

namespace BBWM.Demo.Northwind.DTO;

public class SearchCustomerDTO : IDTO
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Company Name
    /// </summary>
    public string CompanyName { get; set; }
}
