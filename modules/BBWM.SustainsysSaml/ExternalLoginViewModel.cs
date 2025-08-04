using System.ComponentModel.DataAnnotations;

namespace BBWM.SustainsysSaml;

public class ExternalLoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
