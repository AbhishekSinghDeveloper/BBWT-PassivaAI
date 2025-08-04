using System.ComponentModel.DataAnnotations;

namespace BBWT.Server.Models.AccountViewModels;

public class ForgotPasswordViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
