using BBWM.AWS.EventBridge;
using BBWM.AWS.EventBridge.DTO;
using BBWM.AWS.EventBridge.Interfaces;
using BBWM.Core.Services;

namespace BBWM.Demo.EventBridge;

internal sealed class FailureJobMetadata : IEventBridgeJobMetadata<FailureJob>
{
    public string JobId => "FailureDemoJob";

    public string JobDescription =>
        "Demo Event Bridge job. This job will increase the ShippedDate by one day of the demo " +
        "Order with date 21/09/1813. If the increased day is an odd number then it will fail " +
        "otherwise it will complete successfully.";

    public List<JobParameterInfo> Parameters => new()
    {
        new()
        {
            Name = "Increment",
            Required = true,
            Description = "Value used to increment the ShippedDate day."
        },
        new()
        {
            Name = "Fail On Odd days",
            Required = false,
            Description = "If true fails on odd days, otherwise on even days."
        }
    };
}

public class FailureJob : IEventBridgeJob
{
    private static readonly DateTime orderDate = new(1813, 9, 21, 0, 0, 0);
    private readonly IDataService<IDemoDataContext> dataService;

    public FailureJob(IDataService<IDemoDataContext> dataService) => this.dataService = dataService;

    public async Task RunAsync(IEnumerable<AwsEventBridgeJobParameterDTO> @params, CancellationToken ct)
    {
        var order = await OrderServiceHelper.FindOrCreateOrderAsync(dataService, orderDate, ct);
        await OrderServiceHelper.IncreaseShippedDateAndSaveAsync(dataService, order, ct);

        if (order.ShippedDate.Value.Day % 2 == 1)
        {
            throw new Exception(
                $"ShippedDate cannot have and odd day number (day is: {order.ShippedDate.Value.Day}).");
        }
    }
}
