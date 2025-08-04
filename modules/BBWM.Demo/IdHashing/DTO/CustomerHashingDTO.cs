using BBWM.Core.DTO;

namespace BBWM.Demo.IdHashing.DTO;

public class CustomerHashingDTO : IDTO
{
    public int Id { get; set; }

    public string Code { get; set; }

    public string CompanyName { get; set; }
}
