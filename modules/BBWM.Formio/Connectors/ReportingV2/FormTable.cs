namespace BBWM.FormIO.Connectors.ReportingV2;

public class FormTable
{
    public string FriendlyName { get; set; }
    public string TableName { get; set; }
    public string SourceTableName { get; set; }
    public string SourceDefinitionField { get; set; }
    public int SourceDefinitionValue { get; set; }
    public List<FormColumn> Columns { get; set; }
}