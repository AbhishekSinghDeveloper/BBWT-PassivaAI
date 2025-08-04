using BBWM.Core.DTO;

namespace BBWM.DataProcessing.Test;

/// <summary>
/// DTO of Employee
/// </summary>
public class EmployeeDTO : IDTO
{
    /// <summary>
    /// ID
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Name
    /// </summary>
    public string Name { get; set; }

    /// <summary>
    /// Age
    /// </summary>
    public int Age { get; set; }

    /// <summary>
    /// Phone number
    /// </summary>
    public string Phone { get; set; }

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; }

    /// <summary>
    /// Birthday
    /// </summary>
    public DateTimeOffset RegistrationDate { get; set; }

    /// <summary>
    /// Job role
    /// </summary>
    public string JobRole { get; set; }
}
