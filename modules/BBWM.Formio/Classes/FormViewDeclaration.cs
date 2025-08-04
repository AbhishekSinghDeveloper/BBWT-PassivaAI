namespace BBWM.FormIO.Classes;

public class FormViewDeclaration
{
    public string ViewName { get; set; } = null!;
    public List<FormViewTableItem> TableItems { get; set; } = new();
    public List<FormViewColumnItem> SelectionItems { get; set; } = new();
    public List<FormViewFiltrationRule> FiltrationRules { get; set; } = new();

    public string? Sql
    {
        get
        {
            if (ViewName is not { Length: > 0 } viewName) return null;

            var tables = TableItems.Select(item => item.Sql).Where(sql => !string.IsNullOrEmpty(sql)).ToList();
            var columns = SelectionItems.Select(item => item.Sql).Where(sql => !string.IsNullOrEmpty(sql)).ToList();
            var filters = FiltrationRules.Select(rule => rule.Sql).Where(sql => !string.IsNullOrEmpty(sql)).ToList();

            if (tables.Count == 0 || columns.Count == 0) return null;

            var sql = $"CREATE OR REPLACE VIEW {viewName} AS SELECT {string.Join(", ", columns)} FROM {string.Join(", ", tables)}";

            return filters.Count == 0 ? sql : $"{sql} WHERE {string.Join(" AND ", filters)}";
        }
    }
}