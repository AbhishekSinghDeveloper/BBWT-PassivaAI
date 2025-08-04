using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace BBWM.Core.Membership.DTO;

public class Enabling2FADTO
{
    [Required]
    [StringLength(7, MinimumLength = 6)]
    [DataType(DataType.Text)]
    public string Code { get; set; }

    [ReadOnly(true)]
    public string SharedKey { get; set; }

    public string AuthenticatorUri { get; set; }
}
