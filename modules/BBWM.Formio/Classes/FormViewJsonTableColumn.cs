namespace BBWM.FormIO.Classes;

public class FormViewJsonTableColumn
{
    public string Name { get; set; } = null!;
    public string Path { get; set; } = null!;
    public string Type { get; set; } = null!;

    public string? Sql
        => Name is not { Length: > 0 } name ||
           Path is not { Length: > 0 } path ||
           Type is not { Length: > 0 } type
            ? null
            : $"{name} {type} PATH '${path}'";
}