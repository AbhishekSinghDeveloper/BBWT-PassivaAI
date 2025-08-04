namespace BBWM.Core.Security;

public class CSPViolationReportBodyDTO
{
    public CSPViolationReportDTO CspReport { get; set; }
}

public class CSPViolationReportDTO
{
    public string DocumentUri { get; set; }

    public string Referrer { get; set; }

    public string ViolatedDirective { get; set; }

    public string EffectiveDirective { get; set; }

    public string OriginalPolicy { get; set; }

    public string Disposition { get; set; }

    public string BlockedUri { get; set; }

    public int LineNumber { get; set; }

    public int ColumnNumber { get; set; }

    public string SourceFile { get; set; }

    public int StatusCode { get; set; }

    public string ScriptSample { get; set; }
}
