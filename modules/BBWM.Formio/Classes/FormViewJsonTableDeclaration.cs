namespace BBWM.FormIO.Classes;

public class FormViewJsonTableDeclaration : FormViewTableItem
{
    public string Path { get; set; } = null!;
    public string ColumnName { get; set; } = null!;
    public FormViewJsonTableOrdinal? Ordinal { get; set; }
    public FormViewJsonTableDeclaration? NestedDeclaration { get; set; }
    public List<FormViewJsonTableColumn> Columns { get; set; } = new();

    private string? InnerDeclaration
    {
        get
        {
            if (Path is not { Length: > 0 } path) return null;

            var columns = Columns.Select(column => column.Sql).Where(sql => !string.IsNullOrEmpty(sql)).ToList();

            if (Ordinal?.Sql is { Length: > 0 } ordinalDeclaration)
                columns.Add(ordinalDeclaration);

            if (NestedDeclaration?.InnerDeclaration is { Length: > 0 } nestedDeclaration)
                columns.Add($"NESTED PATH {nestedDeclaration}");

            return columns.Count == 0 ? null : $"'${path}' COLUMNS ({string.Join(", ", columns)})";
        }
    }

    public override string? Sql
        => Alias is not { Length: > 0 } alias ||
           ColumnName is not { Length: > 0 } columnName ||
           InnerDeclaration is not { Length: > 0 } innerDeclaration
            ? null
            : $"JSON_TABLE({columnName}, {innerDeclaration}) AS {alias}";
}