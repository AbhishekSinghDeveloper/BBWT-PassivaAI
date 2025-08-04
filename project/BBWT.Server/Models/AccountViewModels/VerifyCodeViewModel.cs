using System.ComponentModel.DataAnnotations;

namespace BBWT.Server.Models.AccountViewModels;

public class VerifyCodeViewModel
{
    [Required]
    public string Provider { get; set; }

    [Required]
    public string Code { get; set; }

    public string ReturnUrl { get; set; }

    [Display(Name = "Remember this browser?")]
    public bool RememberBrowser { get; set; }
}
