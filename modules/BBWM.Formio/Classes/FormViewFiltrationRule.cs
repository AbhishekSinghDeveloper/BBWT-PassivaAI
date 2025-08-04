namespace BBWM.FormIO.Classes;

public class FormViewFiltrationRule
{
    public string Operator { set; get; } = "=";
    public string ColumnName { set; get; } = null!;
    public string ColumnValue { set; get; } = null!;

    public string? Sql
        => Operator is not { Length: > 0 } @operator ||
           ColumnName is not { Length: > 0 } columnId ||
           ColumnValue is not { Length: > 0 } columnValue
            ? null
            : $"{columnId} {@operator} {columnValue}";
}