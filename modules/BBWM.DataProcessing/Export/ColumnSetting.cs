namespace BBWM.DataProcessing.Export;

public class ColumnSetting<T>
{
    public string Header { get; set; }
    public Func<T, string> GetValue { get; set; }
}
