using BBWM.Core.DTO;

namespace BBWM.Demo.IdHashing.DTO;

public class SimpleOrderHashingDTO : IDTO
{
    public int Id { get; set; }

    public string CustomerCompanyName { get; set; }

    public bool IsPaid { get; set; }
}
