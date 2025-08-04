namespace BBWM.FormIO.Classes;

public class FormViewColumnExpression : FormViewColumnItem
{
    public string Expression { set; get; } = null!;

    public override string? Sql
        => Expression is not { Length: > 0 } expression ||
           Alias is not { Length: > 0 } alias
            ? null
            : $"{expression} AS {alias}";
}