using BBWM.Core.Data;

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BBWM.Messages.Templates;

/// <summary>
/// Email template
/// </summary>
[Table("EmailTemplates")]
public class EmailTemplate : IAuditableEntity
{
    public int Id { get; set; }

    /// <summary>
    /// Code
    /// </summary>
    [Required, MaxLength(32)]
    public string Code { get; set; }

    /// <summary>
    /// Title
    /// </summary>
    [Required]
    public string Title { get; set; }

    /// <summary>
    /// Determines if this is a system template
    /// </summary>
    public bool IsSystem { get; set; }

    /// <summary>
    /// Email's From
    /// </summary>
    [Required, MaxLength(256)]
    public string From { get; set; }

    /// <summary>
    /// Email's Subject
    /// </summary>
    [Required]
    public string Subject { get; set; }

    /// <summary>
    /// Email's Body
    /// </summary>
    [Required]
    public string Body { get; set; }
}
