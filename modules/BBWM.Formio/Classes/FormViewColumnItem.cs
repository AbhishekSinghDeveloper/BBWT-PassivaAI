namespace BBWM.FormIO.Classes;

public abstract class FormViewColumnItem
{
    public string? FormLabel { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsForeignKey { get; set; }
    public string Type { get; set; } = null!;

    public virtual string Alias { get; set; } = null!;
    public virtual string? Sql => null;
}