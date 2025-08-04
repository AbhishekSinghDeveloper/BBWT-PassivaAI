using Autofac;
using Autofac.Features.OwnedInstances;

using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;
using BBWM.Core.Services;

using Microsoft.Extensions.Logging;

namespace BBWM.AWS.EventBridge;

public sealed class TrackingJobContext : IDisposable
{
    private readonly ILifetimeScope _lifetimeScope;
    private readonly Type _jobType;

    public IAwsEventBridgeClientFactory ClientFactory { get; private set; }

    public IAwsEventBridgeJobService JobService { get; private set; }

    public IDataService DataService { get; private set; }

    public IEventBridgeJobErrorHandler ErrorHandler { get; }

    public IEventBridgeJob Job => _lifetimeScope.Resolve(_jobType) as IEventBridgeJob;

    public IEventBridgeJobMetadata JobMetadata
        => _lifetimeScope.Resolve(typeof(IEventBridgeJobMetadata<>).MakeGenericType(_jobType)) as IEventBridgeJobMetadata;

    public List<AwsEventBridgeJobParameterDTO> Parameters { get; }

    public ILogger<IJobExecutionWrapper> Logger { get; }

    public delegate Owned<TrackingJobContext> Factory(Type jobType, List<AwsEventBridgeJobParameterDTO> parameters);

    public TrackingJobContext(
        Type jobType,
        List<AwsEventBridgeJobParameterDTO> parameters,
        ILifetimeScope lifetimeScope,
        IAwsEventBridgeClientFactory clientFactory,
        IAwsEventBridgeJobService jobService,
        IDataService dataService,
        IEventBridgeJobErrorHandler errorHandler,
        ILogger<IJobExecutionWrapper> logger)
    {
        _jobType = jobType;
        _lifetimeScope = lifetimeScope;
        Parameters = parameters;
        ClientFactory = clientFactory;
        JobService = jobService;
        DataService = dataService;
        ErrorHandler = errorHandler;
        Logger = logger;
    }

    public void Dispose() => _lifetimeScope?.Dispose();
}
