namespace BBWM.DbDoc.DbMacros;

public class DbPathMacroDefinition
{
    public string Alias { get; set; }

    public string Description { get; set; }
    public string SourceTable { get; set; }

    public string TargetTable { get; set; }

    public string ExpectedTargetReferencingColumn { get; set; }
}