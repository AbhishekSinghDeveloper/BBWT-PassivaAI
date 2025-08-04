using BBWM.Core;
using BBWM.Core.Data;
using BBWM.Core.Membership.Model;

namespace BBF.Reporting.Core.DbModel;

public class WidgetSource : IAuditableEntity<Guid>
{
    public Guid Id { get; set; }

    public bool IsDraft { get; set; }

    /// <summary>
    /// Grid/Chart/ControlSet/...
    /// </summary>
    public string WidgetType { get; set; } = null!;

    public string? Name { get; set; }

    public string? Title { get; set; }

    /// <summary>
    /// A unique code which identifies widget within code developing environment and is used
    /// to embed widget's HTML block into HTML code markup.
    /// E.g. HTML: ...reporting-widget code="invoices-2024-grid" ...
    /// </summary>
    public string? Code { get; set; }

    public DateTime CreatedOn { get; set; }

    // Foreign keys and navigational properties.
    public Guid? ReleaseWidgetId { get; set; }

    public WidgetSource? ReleaseWidget { get; set; }

    public string? OwnerId { get; set; }

    public User? Owner { get; set; }

    public ICollection<Organization> Organizations { get; set; } = new List<Organization>();

    /// <summary>
    /// A Display rule field describes an expression based on a variable rule to define
    /// a condition for showing/hiding the widget
    /// ([#variable] [operator] [operand], e.g. "#orgID > 0")
    /// </summary>
    public int? DisplayRuleId { get; set; }

    [DoNotAutoignore] public VariableRule? DisplayRule { get; set; }
}