using BBWM.Core.Web;

using System.Text.Json;

namespace BBWM.Menu.DTO;

public class MenuDTO
{
    public MenuDTO() => Children = new List<MenuDTO>();

    public MenuDTO(Route route, IEnumerable<MenuDTO> children = null)
    {
        Label = route.Title;
        RouterLink = route.Path;
        if (children is null) Children = new List<MenuDTO>();
    }

    public MenuDTO(Route route, string icon)
    {
        Icon = icon;
        Label = route.Title;
        RouterLink = route.Path;
        Children = new List<MenuDTO>();
    }

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
    /// Children items.
    /// </summary>
    public ICollection<MenuDTO> Children { get; set; }

    public override string ToString()
    {
        return JsonSerializer.Serialize(this);
    }
}