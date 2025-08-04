using System.ComponentModel.DataAnnotations;

namespace BBWM.Core.Membership.DTO;

public class Disabling2FADTO
{
    [Required]
    [StringLength(7, MinimumLength = 6)]
    [DataType(DataType.Text)]
    public string Code { get; set; }
}
