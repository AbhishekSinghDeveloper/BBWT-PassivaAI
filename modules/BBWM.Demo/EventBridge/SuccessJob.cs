using BBWM.AWS.EventBridge;
using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;
using BBWM.Core.Services;

namespace BBWM.Demo.EventBridge;

internal sealed class SuccessJobMetadata : IEventBridgeJobMetadata<SuccessJob>
{
    public string JobId => "SuccessDemoJob";

    public string JobDescription =>
        "Demo Event Bridge job. This job will increase the ShippedDate by one day of the demo " +
        "Order with date 11/12/1812. Hopefully, it will always complete successfully.";

    public List<JobParameterInfo> Parameters => new()
    {
        new()
        {
            Name = "Increment",
            Required = true,
            Description = "Value used to increment the ShippedDate day."
        }
    };
}

public class SuccessJob : IEventBridgeJob
{
    private static readonly DateTime orderDate = new(1812, 12, 11, 0, 0, 0);
    private readonly IDataService<IDemoDataContext> dataService;

    public SuccessJob(IDataService<IDemoDataContext> dataService) => this.dataService = dataService;

    public async Task RunAsync(IEnumerable<AwsEventBridgeJobParameterDTO> @params, CancellationToken ct)
    {
        var order = await OrderServiceHelper.FindOrCreateOrderAsync(dataService, orderDate, ct);
        await OrderServiceHelper.IncreaseShippedDateAndSaveAsync(dataService, order, ct);
    }
}
