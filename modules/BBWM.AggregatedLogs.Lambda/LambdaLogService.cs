using BBWM.AggregatedLogs.Lambda.DTO;

namespace BBWM.AggregatedLogs.Lambda
{
    public class LambdaLogService : ILambdaLogService
    {
        private readonly ILogContext _logContext;
        private readonly ILogParser _logParser;

        public LambdaLogService(ILogContext logContext, ILogParser logParser)
        {
            _logContext = logContext;
            _logParser = logParser;
        }

        public async Task ProcessLogs(EventDTO input)
        {
            if (input.awslogs.data != null)
            {
                var entries = await _logParser.Parse(input);
                _logContext.Logs.AddRange(entries);
                await _logContext.SaveChangesAsync();
            }
        }
    }
}
