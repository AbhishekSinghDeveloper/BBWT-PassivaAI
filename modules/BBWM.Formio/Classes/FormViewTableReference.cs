namespace BBWM.FormIO.Classes;

public class FormViewTableReference : FormViewTableItem
{
    public string Name { set; get; } = null!;
    public string? TableAlias { set; get; }

    public override string Alias
    {
        get => TableAlias ?? Name;
        set => TableAlias = value;
    }

    public override string? Sql
        => Name is not { Length: > 0 } name
            ? null
            : TableAlias is not { Length: > 0 } alias
                ? name
                : $"{name} AS {alias}";
}