using BBWM.Core.Data;

using System.ComponentModel.DataAnnotations;

namespace BBWM.Messages.Templates;

/// <summary>
/// Email template parameter
/// </summary>
public class EmailTemplateParameter : IAuditableEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Code
    /// </summary>
    [Required, MaxLength(64)]
    public string Title { get; set; }

    /// <summary>
    /// Description
    /// </summary>
    public string Notes { get; set; }
}
