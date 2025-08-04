using BBWM.Core.Data;
using BBWM.Core.Data.Attributes;

using System.ComponentModel.DataAnnotations.Schema;

namespace BBWM.DataProcessing.Test;

/// <summary>
/// Demo employee entity
/// </summary>
[Table("Employees")]
[AllowDeleteAll]
public class Employee : IEntity
{
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
    /// Registration date
    /// </summary>
    public DateTimeOffset RegistrationDate { get; set; }

    /// <summary>
    /// Job role
    /// </summary>
    public string JobRole { get; set; }
}
