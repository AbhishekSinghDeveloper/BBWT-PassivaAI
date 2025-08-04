namespace BBWM.FormIO.Classes;

public class FormViewColumnReference : FormViewColumnItem
{
    public string Name { get; set; } = null!;
    public string? ColumnAlias { get; set; }

    public override string Alias
    {
        get => ColumnAlias ?? Name;
        set => ColumnAlias = value;
    }

    public override string? Sql
        => Name is not { Length: > 0 } name
            ? null
            : ColumnAlias is not { Length: > 0 } alias
                ? name
                : $"{name} AS {alias}";
}