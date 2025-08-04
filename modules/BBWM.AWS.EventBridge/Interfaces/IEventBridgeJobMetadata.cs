namespace BBWM.AWS.EventBridge.Interfaces;

/// <summary>
/// Marker interface to separately define the <see cref="TJob"/>'s metadata
/// </summary>
///
/// <remarks>
/// Any call to <see cref="IAwsEventBridgeJobService.RegisterJob{TJob}"/> for a job which metadata
/// cannot be found will be ignored.
/// </remarks>
/// <typeparam name="TJob">The job's type</typeparam>
public interface IEventBridgeJobMetadata<TJob> : IEventBridgeJobMetadata
    where TJob : IEventBridgeJob
{ }

public interface IEventBridgeJobMetadata
{
    string JobId { get; }
    string JobDescription { get; }
    List<JobParameterInfo> Parameters { get; }
}