namespace BBWM.FormIO.Connectors.ReportingV2;

public class FormColumn
{
    public string Name { get; set; }
    public string Type { get; set; }
    public string Path { get; set; }
    public bool IsPrimaryKey { get; set; }
    public bool IsForeingKey { get; set; }
}