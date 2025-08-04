namespace BBWM.ReportProblem;

public interface IReportProblemService
{
    Task Send(ReportProblemDTO reportProblem, string userAgent, string baseUrl);
    Task AutoSend(Exception exception);
    Task AutoSend(ErrorLogDTO errorLogDTO);
}
