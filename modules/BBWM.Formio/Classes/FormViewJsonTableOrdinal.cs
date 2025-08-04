namespace BBWM.FormIO.Classes;

public class FormViewJsonTableOrdinal
{
    public string Name { get; set; } = null!;

    public string? Sql
        => Name is not { Length: > 0 } name
            ? null
            : $"{name} FOR ORDINALITY";
}