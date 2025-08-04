using System.ComponentModel.DataAnnotations;

namespace BBWM.Demo.SimulateError;

public class SimulateBadRequestDTO
{
    [Required, StringLength(4)]
    public string FirstField { get; set; }

    [Required]
    public string SecondField { get; set; }
}
