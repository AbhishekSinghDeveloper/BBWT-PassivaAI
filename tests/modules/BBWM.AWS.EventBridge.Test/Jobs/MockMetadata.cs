using BBWM.AWS.EventBridge;
using BBWM.AWS.EventBridge.Interfaces;

class MockMetadata<TJob> : IEventBridgeJobMetadata<TJob>
       where TJob : IEventBridgeJob
{
    public MockMetadata(string jobId, string description, List<JobParameterInfo> @params)
    {
        JobId = jobId;
        JobDescription = description;
        Parameters = @params;
    }

    public string JobId { get; }

    public string JobDescription { get; }

    public List<JobParameterInfo> Parameters { get; }
}