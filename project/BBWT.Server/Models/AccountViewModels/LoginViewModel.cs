using System.ComponentModel.DataAnnotations;

namespace BBWT.Server.Models.AccountViewModels;

public class LoginViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [DataType(DataType.Password)]
    public string Password { get; set; }

    public string Fingerprint { get; set; }

    public string Browser { get; set; }

    public string Ip { get; set; }
}
