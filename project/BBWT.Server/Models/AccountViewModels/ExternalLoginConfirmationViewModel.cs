using System.ComponentModel.DataAnnotations;

namespace BBWT.Server.Models.AccountViewModels;

public class ExternalLoginConfirmationViewModel
{
    [Required]
    [EmailAddress]
    public string Email { get; set; }
}
