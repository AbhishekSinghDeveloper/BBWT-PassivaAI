using Amazon.Lambda.Core;

// Uncommit when used in a separate project
//[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace BBWM.AggregatedLogs.TestLambda;

public class Function
{
    public void FunctionHandler(string input, ILambdaContext context)
    {
        switch (input)
        {
            case "Error":
                try
                {
                    throw new ApplicationException("Test exception");
                }
                catch (Exception ex)
                {
                    context.Logger.LogError($"Error: {ex}");
                }
                break;
            case "Info":
                context.Logger.LogInformation("Test info");
                break;
            case "Warning":
                context.Logger.LogWarning("Test warning");
                break;
            case "Critical":
                context.Logger.LogCritical("Test critical");
                break;
            case "Debug":
                context.Logger.LogDebug("Test debug");
                break;
            case "Trace":
                context.Logger.LogTrace("Test trace");
                break;
            case "Line":
                context.Logger.LogLine("Test line");
                break;
            case "Unhandled":
                throw new ApplicationException("Test exception");
        }
    }
}
