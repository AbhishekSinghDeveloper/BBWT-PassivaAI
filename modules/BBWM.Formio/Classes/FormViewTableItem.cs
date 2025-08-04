namespace BBWM.FormIO.Classes;

public abstract class FormViewTableItem
{
    public virtual string Alias { get; set; } = null!;
    public virtual string? Sql => null;
}