using BBWM.Core.Data;

using System.ComponentModel.DataAnnotations.Schema;

namespace BBWM.Menu.Db;

/// <summary>
/// Menu item entity.
/// </summary>
[Table("Menu")]
public class MenuItem : IAuditableEntity
{
    /// <summary>
    /// Identifier int the DB.
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// Label displayed in the UI.
    /// </summary>
    public string Label { get; set; }

    /// <summary>
    /// Sequential number in the list.
    /// </summary>
    public int Index { get; set; }

    /// <summary>
    /// Angular "routerLink" directive value.
    /// </summary>
    public string RouterLink { get; set; }

    /// <summary>
    /// External link.
    /// </summary>
    public string Href { get; set; }

    /// <summary>
    /// CSS classes applying to the "li" tag of the list.
    /// </summary>
    public string Classes { get; set; }

    /// <summary>
    /// CSS classes applying to the left side icon.
    /// </summary>
    public string Icon { get; set; }

    /// <summary>
    /// Name of the custom handler that should be applied on the front end.
    /// </summary>
    public string CustomHandler { get; set; }

    /// <summary>
    /// Defines whether if item is hidden. 
    /// </summary>
    public bool? Hidden { get; set; }

    /// <summary>
    /// Defines whether if item is disabled. 
    /// </summary>
    public bool? Disabled { get; set; }


    /// <summary>
    /// Identifier of the parent item.
    /// </summary>
    public int? ParentId { get; set; }

    /// <summary>
    /// Parent item.
    /// </summary>
    [ForeignKey("ParentId")]
    public virtual MenuItem Parent { get; set; }


    /// <summary>
    /// Children items.
    /// </summary>
    public virtual ICollection<MenuItem> Children { get; set; }
}
