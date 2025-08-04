using BBWM.Core.DTO;

namespace BBWM.Demo.IdHashing.DTO;

public class OrderHashingDTO : IDTO
{
    public int Id { get; set; }

    public string CustomerCode { get; set; }

    public string CustomerCompanyName { get; set; }

    public DateTime? OrderDate { get; set; }

    public DateTime? RequiredDate { get; set; }

    public DateTime? ShippedDate { get; set; }

    public bool IsPaid { get; set; }

    public bool HasResellerItems { get; set; }

    public int? CustomerId { get; set; }

    public CustomerHashingDTO Customer { get; set; }

    public List<OrderDetailHashingDTO> OrderDetails { get; set; } = new List<OrderDetailHashingDTO>();
}
