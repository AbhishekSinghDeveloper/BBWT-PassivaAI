using BBWM.Core.DTO;

namespace BBWM.Demo.IdHashing.DTO;

public class OrderDetailHashingDTO : IDTO
{
    public int Id { get; set; }

    public string ProductTitle { get; set; }

    public decimal Quantity { get; set; }

    public decimal Price { get; set; }

    public bool IsReseller { get; set; }
}
