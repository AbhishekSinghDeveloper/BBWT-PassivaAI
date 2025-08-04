using BBWM.FormIO.Enums;

namespace BBWM.FormIO.Classes;

public class FormFieldDataUpdate
{
    public string Key { get; set; } = null!;
    public string Type { get; set; } = null!;
    public dynamic? Value { get; set; }
    public FormFieldChangeAction Action { get; set; }
    public ICollection<FormFieldDataUpdate> Updates { get; set; } = new List<FormFieldDataUpdate>();
}